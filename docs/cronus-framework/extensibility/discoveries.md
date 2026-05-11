# Discoveries

A discovery is a class that Cronus runs at boot to decide which services should be added to the host's `IServiceCollection`. Every satellite package ships one or more discoveries; your own code can ship them too. This is how Cronus is assembled out of a shared core plus pluggable satellites without any of them having a compile-time reference to the host's `Program.cs`.

## How the pieces fit together

The moving parts live under [`Elders.Cronus.Discoveries`](https://github.com/Elders/Cronus/tree/master/src/Elders.Cronus/Discoveries). They are, in rough dependency order:

* [`DiscoveryBase<TCronusService>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Discoveries/DiscoveryBase.cs) — abstract base that implements `IDiscovery<TCronusService>` and defines the one method you override, `DiscoverFromAssemblies(DiscoveryContext)`. Only the generic form exists; there is no non-generic `DiscoveryBase`.
* [`IDiscovery<out TCronusService>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Discoveries/IDiscovery.cs) — exposes a `Name` and a `Discover(DiscoveryContext)` method that returns `IDiscoveryResult<TCronusService>`.
* [`DiscoveryContext`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Discoveries/DiscoveryContext.cs) — the read-only state the scanner hands to every discovery. It holds `Assemblies` (the set of assemblies Cronus has loaded), `Configuration` (the `IConfiguration` passed to `AddCronus`), a derived `Types` enumeration, and the helper `FindService<TService>()` which scans the assemblies for concrete implementations of `TService`. **It does not hold `IServiceCollection`** — that is the job of `CronusServicesProvider`.
* [`IDiscoveryResult<out T>` and `DiscoveryResult<T>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Discoveries/IDiscoveryResult.cs) — the result a discovery returns. It wraps an `IEnumerable<DiscoveredModel>` plus an optional `Action<IServiceCollection>` for registrations that don't fit the `DiscoveredModel` shape (typically `AddOptions` calls). Both are generic only; there is no non-generic form.
* [`DiscoveredModel`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Discoveries/IDiscoveryResult.cs) — extends the standard `Microsoft.Extensions.DependencyInjection.ServiceDescriptor` with two flags: `CanOverrideDefaults` (use `Services.Replace`) and `CanAddMultiple` (use `Services.Add`). If neither flag is set the model is applied with `Services.TryAdd`, i.e. it registers the service only if nothing else has claimed it. The class is non-generic; there is no `DiscoveredModel<T>`.
* [`DiscoveryScanner`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Discoveries/DiscoveryScanner.cs) — the reflection-based invoker. Its only public member is `IEnumerable<IDiscoveryResult<object>> Scan(DiscoveryContext context)`, which returns one result per discovery type it finds.
* [`CronusServicesProvider`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Discoveries/CronusServicesProvider.cs) — the object that applies a discovery result to the `IServiceCollection`. It calls the `AddServices` action first, then walks `Models` and dispatches each one to `Services.Add`, `Services.Replace`, or `Services.TryAdd` depending on the two flags above.

## What the scanner actually does

`DiscoveryScanner.Scan` looks like this:

```csharp
public IEnumerable<IDiscoveryResult<object>> Scan(DiscoveryContext context)
{
    IEnumerable<Type> allTypes = context.Assemblies
           .SelectMany(asm => asm
               .GetLoadableTypes()
               .Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery<object>).IsAssignableFrom(type)));

    IEnumerable<IDiscovery<object>> discoveries = allTypes
        .Where(candidate => allTypes.Where(t => t.BaseType == candidate).Any() == false) // filter out discoveries which inherit from each other. We remove the base discoveries
        .Select(dt => (IDiscovery<object>)FastActivator.CreateInstance(dt));

    foreach (var discovery in discoveries)
    {
        logger.LogInformation("Discovered {name}.", discovery.Name);

        yield return discovery.Discover(context);
    }
}
```

Two details are worth calling out:

1. The filter `t.BaseType == candidate` removes any discovery that has a subclass elsewhere in the assemblies. That is how the `EventStoreDiscovery` in Cronus core is replaced by `CassandraEventStoreDiscovery` in the satellite — only the subclass runs.
2. Discoveries are instantiated by `FastActivator.CreateInstance` with no constructor parameters. A discovery class must have a public parameterless constructor.

## A canonical publisher discovery

Here is the RabbitMQ publisher discovery in full. It registers the three publisher lanes Cronus has (generic `IPublisher<>`, public-event publisher, signal publisher) and marks each as overriding the in-memory defaults:

```csharp
public class RabbitMqPublisherDiscovery : DiscoveryBase<IPublisher<IMessage>>
{
    protected override DiscoveryResult<IPublisher<IMessage>> DiscoverFromAssemblies(DiscoveryContext context)
    {
        return new DiscoveryResult<IPublisher<IMessage>>(GetModels(), services => services
                                                                                    .AddOptions<RabbitMqOptions, RabbitMqOptionsProvider>()
                                                                                    .AddOptions<PublicRabbitMqOptions, PublicRabbitMqOptionsProvider>());
    }

    IEnumerable<DiscoveredModel> GetModels()
    {
        yield return new DiscoveredModel(typeof(BoundedContextRabbitMqNamer), typeof(BoundedContextRabbitMqNamer), ServiceLifetime.Singleton);
        yield return new DiscoveredModel(typeof(PublicMessagesRabbitMqNamer), typeof(PublicMessagesRabbitMqNamer), ServiceLifetime.Singleton);
        yield return new DiscoveredModel(typeof(SignalMessagesRabbitMqNamer), typeof(SignalMessagesRabbitMqNamer), ServiceLifetime.Singleton);

        yield return new DiscoveredModel(typeof(PrivateRabbitMqPublisher<>), typeof(PrivateRabbitMqPublisher<>), ServiceLifetime.Singleton);
        yield return new DiscoveredModel(typeof(PublicRabbitMqPublisher), typeof(PublicRabbitMqPublisher), ServiceLifetime.Singleton);
        yield return new DiscoveredModel(typeof(SignalRabbitMqPublisher), typeof(SignalRabbitMqPublisher), ServiceLifetime.Singleton);

        var publisherModel = new DiscoveredModel(typeof(IPublisher<>), typeof(PrivateRabbitMqPublisher<>), ServiceLifetime.Singleton);
        publisherModel.CanOverrideDefaults = true;
        yield return publisherModel;

        var publicPublisherModel = new DiscoveredModel(typeof(IPublisher<IPublicEvent>), typeof(PublicRabbitMqPublisher), ServiceLifetime.Singleton);
        publicPublisherModel.CanOverrideDefaults = true;
        yield return publicPublisherModel;

        var signalPublisherModel = new DiscoveredModel(typeof(IPublisher<ISignal>), typeof(SignalRabbitMqPublisher), ServiceLifetime.Singleton);
        signalPublisherModel.CanOverrideDefaults = true;
        yield return signalPublisherModel;

        yield return new DiscoveredModel(typeof(RabbitMqInfrastructure), typeof(RabbitMqInfrastructure), ServiceLifetime.Singleton);

        yield return new DiscoveredModel(typeof(ConnectionResolver), typeof(ConnectionResolver), ServiceLifetime.Singleton);
        yield return new DiscoveredModel(typeof(PublisherChannelResolver), typeof(PublisherChannelResolver), ServiceLifetime.Singleton);
    }
}
```

Source: [`Cronus.Transport.RabbitMQ/src/Elders.Cronus.Transport.RabbitMQ/Publisher/RabbitMqPublisherDiscovery.cs`](https://github.com/Elders/Cronus.Transport.RabbitMQ/blob/master/src/Elders.Cronus.Transport.RabbitMQ/Publisher/RabbitMqPublisherDiscovery.cs).

What to notice:

* The type parameter on `DiscoveryBase<IPublisher<IMessage>>` is the *conceptual service* the discovery owns — it is informational, not a constraint. The scanner does not act on it.
* `AddServices` is where the `AddOptions` calls live, because `AddOptions` does not fit the `DiscoveredModel` shape (it registers three services — `IConfigureOptions`, `IOptionsChangeTokenSource`, `IOptionsFactory`).
* The publisher models set `CanOverrideDefaults = true` because they are meant to replace the in-memory publishers registered by `InMemoryPublisherDiscovery`.

## Extending an existing discovery

A discovery can inherit from another discovery and extend its model set. The Cassandra event store does exactly that:

```csharp
public class CassandraEventStoreDiscovery : EventStoreDiscovery
{
    protected override DiscoveryResult<IEventStore> DiscoverFromAssemblies(DiscoveryContext context)
    {
        IEnumerable<DiscoveredModel> models = base.DiscoverFromAssemblies(context).Models
            .Concat(GetModels(context))
            .Concat(DiscoverCassandraTableNameStrategy(context));

        return new DiscoveryResult<IEventStore>(models, services => services.AddOptions<CassandraProviderOptions, CassandraProviderOptionsProvider>());
    }

    IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
    {
        yield return new DiscoveredModel(typeof(IEventStore<>), typeof(CassandraEventStore<>), ServiceLifetime.Transient) { CanOverrideDefaults = true };
        yield return new DiscoveredModel(typeof(IEventStore), typeof(CassandraEventStore), ServiceLifetime.Transient) { CanOverrideDefaults = true };
        // ...
    }
}
```

Source: [`Cronus.Persistence.Cassandra/src/Elders.Cronus.Persistence.Cassandra/CassandraEventStoreDiscovery.cs`](https://github.com/Elders/Cronus.Persistence.Cassandra/blob/master/src/Elders.Cronus.Persistence.Cassandra/CassandraEventStoreDiscovery.cs).

Because `CassandraEventStoreDiscovery` inherits from `EventStoreDiscovery`, the scanner's "filter out base discoveries" rule kicks in: `EventStoreDiscovery` will not run on its own — only the subclass runs — which is exactly what you want.

## Which assemblies are scanned

The assemblies fed to every `DiscoveryContext` come from [`AssemblyLoader`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/AssemblyLoader.cs). Its static constructor runs once, on first access, and does the following:

1. Takes `Assembly.GetEntryAssembly().Location` and derives the directory the host was launched from.
2. Enumerates every `*.dll` and `*.exe` file in that directory.
3. Skips a hard-coded list of unmanaged runtime files (`sni.dll`, `coreclr.dll`, `clrjit.dll`, and so on) and anything whose file name contains one of the wildcards `microsoft`, `api-ms`, `sos_`, `mscordaccore`, `mscor`.
4. For every remaining file, checks if the assembly is already loaded in `AppDomain.CurrentDomain.GetAssemblies()`; if not, loads it with `AssemblyLoadContext.Default.LoadFromAssemblyPath`.
5. Adds the loaded assemblies to the static `Assemblies` dictionary keyed by full name.

`DiscoveryContext.Assemblies` is this dictionary. If the directory the host runs from does not contain your satellite's DLL, the discovery will not run. This is why the recommended hosting pattern is to deploy every satellite package into the host process's output directory.

## Writing your own discovery

A discovery is the right extension point whenever you want to add, replace, or reconfigure services that Cronus will consume at runtime — a custom publisher, a custom event store, a custom handler factory, and so on. The checklist:

1. Create a class that derives from `DiscoveryBase<TCronusService>`, where `TCronusService` is the conceptual service the discovery owns. It is informational; pick whatever type documents intent.
2. Give the class a public parameterless constructor so `FastActivator.CreateInstance` can build it.
3. Override `DiscoverFromAssemblies(DiscoveryContext context)` and return a `DiscoveryResult<TCronusService>` containing:
   * An `IEnumerable<DiscoveredModel>` — one entry per service you want to register.
   * An optional `Action<IServiceCollection>` for registrations that do not fit the `DiscoveredModel` shape (typically `AddOptions<TOptions, TOptionsProvider>`).
4. On each `DiscoveredModel`, set `CanOverrideDefaults = true` if you want to replace a default registration, or `CanAddMultiple = true` if you want the service to be registered alongside others. Leave both false if you only want to fill in when nothing else has.
5. Make sure the assembly containing your discovery class is deployed into the host process's directory so `AssemblyLoader` picks it up.

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* a discovery **can** add new services, replace defaults, or register multiple implementations
* a discovery **can** inherit from another discovery to extend its model set — the scanner then runs only the subclass
* a discovery **must** have a public parameterless constructor
* a discovery **must** be deterministic — every run on the same assemblies and configuration produces the same registrations
{% endhint %}

{% hint style="warning" %}
**You should not...**

* a discovery **should not** read external resources (databases, HTTP endpoints) — it runs during `AddCronus`, well before the process is ready for that
* a discovery **should not** resolve services from the container it is about to mutate
* a discovery **should not** assume an execution order relative to other discoveries; use `CanOverrideDefaults` if the final wiring depends on overriding something
{% endhint %}
