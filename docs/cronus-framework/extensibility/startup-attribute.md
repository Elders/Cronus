# `[CronusStartup]` and boot phases

Once `AddCronus` has run — and every discovery has registered its services — Cronus still has to invoke one-time startup code (create Cassandra keyspaces, provision RabbitMQ exchanges, register event-store indices for each tenant, and so on). That one-time code lives in classes that implement [`ICronusStartup`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/ICronusStartup.cs):

```csharp
public interface ICronusStartup
{
    void Bootstrap();
}

public interface ICronusTenantStartup
{
    void Bootstrap();
}
```

`ICronusStartup` runs **once per host**. `ICronusTenantStartup` runs **once per tenant per host** — Cronus creates a scoped `ICronusContext` for each tenant before calling it.

Startup classes are discovered by [`CronusStartupScanner`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/StartupScanner.cs) during `CronusBooter.BootstrapCronus()`. The scanner finds every concrete class that implements the interface, orders them by the phase declared on `[CronusStartup]`, resolves each from the service provider, and calls `Bootstrap()`.

## The `Bootstraps` enum

The phase is declared with [`CronusStartupAttribute`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/CronusStartupAttribute.cs):

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class CronusStartupAttribute : Attribute
{
    public CronusStartupAttribute() : this(Bootstraps.Runtime) { }

    public CronusStartupAttribute(Bootstraps bootstraps)
    {
        Bootstraps = bootstraps;
    }

    public Bootstraps Bootstraps { get; }
}
```

The enum is small and stable. Each value is literally the numeric rank used by the scanner's `OrderBy` — smaller runs earlier.

| Value | Integer | Intent |
| --- | ---: | --- |
| `Environment` | `0` | Prepare the environment for Cronus (set process-wide switches, configure loggers) |
| `ExternalResource` | `10` | Provision external resources such as a database keyspace or a message broker exchange |
| `Configuration` | `20` | Finalise configuration and options |
| `Aggregates` | `30` | One-time work for aggregates |
| `Ports` | `40` | One-time work for ports |
| `Sagas` | `50` | One-time work for sagas |
| `EventStoreIndices` | `55` | Register per-tenant event-store indices (see [`EventStoreIndicesStartup`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/EventStoreIndicesStartup.cs)) |
| `Projections` | `60` | One-time work for projections |
| `Gateways` | `70` | One-time work for gateways |
| `Runtime` | `1000` | Anything else; this is the default when the attribute is omitted |

The values with a `0`, `10`, `20` and `1000` ordering leave room for satellites to slot in between phases without ever colliding with the framework's own numbers. That is deliberate — you can use `(Bootstraps)15` if you really need to run between `ExternalResource` and `Configuration`, because the enum is just an `int` under the hood.

## Not to be confused with discoveries

The `[CronusStartup]` attribute is only for `ICronusStartup` and `ICronusTenantStartup` implementations. **It does not affect discoveries.** Discoveries are found by `DiscoveryScanner`, not by `CronusStartupScanner`, and the scanner does not look at this attribute at all. A discovery runs when `AddCronus` is called; a startup runs later, when `CronusBooter.BootstrapCronus()` is called. Do not put `[CronusStartup(Bootstraps.X)]` on a discovery expecting the phase to apply — it will be silently ignored.

If you need a discovery to run in a particular order relative to others, the right tool is the `CanOverrideDefaults` flag on `DiscoveredModel`, not a bootstrap phase.

## Writing a custom startup

Example — a startup that provisions a third-party resource during the `ExternalResource` phase:

```csharp
[CronusStartup(Bootstraps.ExternalResource)]
public class SearchIndexStartup : ICronusStartup
{
    private readonly ISearchIndexProvisioner provisioner;
    private readonly ILogger<SearchIndexStartup> logger;

    public SearchIndexStartup(ISearchIndexProvisioner provisioner, ILogger<SearchIndexStartup> logger)
    {
        this.provisioner = provisioner;
        this.logger = logger;
    }

    public void Bootstrap()
    {
        logger.LogInformation("Provisioning search index...");
        provisioner.EnsureIndexExists();
    }
}
```

For the class to be resolvable, it must be registered in the container. The idiomatic way is to ship it as part of a discovery in the same assembly:

```csharp
public class SearchIndexDiscovery : DiscoveryBase<ICronusStartup>
{
    protected override DiscoveryResult<ICronusStartup> DiscoverFromAssemblies(DiscoveryContext context)
    {
        return new DiscoveryResult<ICronusStartup>(new[]
        {
            new DiscoveredModel(typeof(SearchIndexStartup), typeof(SearchIndexStartup), ServiceLifetime.Singleton),
            new DiscoveredModel(typeof(ISearchIndexProvisioner), typeof(SearchIndexProvisioner), ServiceLifetime.Singleton)
        });
    }
}
```

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* an `ICronusStartup` **can** provision external resources, register indices, warm caches
* an `ICronusStartup` **must** be safe to run repeatedly — the host may restart
* an `ICronusTenantStartup` **must** be safe to run per tenant, repeatedly
* you **should** pick the smallest phase number that still satisfies your ordering requirements
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **should not** put `[CronusStartup]` on a discovery — it does nothing there
* you **should not** rely on startup phases for fine-grained ordering within a phase; classes with the same rank run in an unspecified order
* you **should not** do heavy, long-running work in a startup — startups block the boot sequence
{% endhint %}
