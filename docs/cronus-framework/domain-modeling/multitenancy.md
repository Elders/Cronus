# Multitenancy in the Cronus Framework

The Cronus framework supports **multitenancy**, enabling a single application instance to serve multiple tenants while ensuring data isolation and security for each. This design allows for efficient resource utilization and simplified maintenance across diverse client bases.

## Key Characteristics of Multitenancy in Cronus

- **Tenant Isolation:** Each tenant's data and configurations are isolated, preventing unauthorized access and ensuring privacy.

- **Dynamic Tenant Management:** Cronus allows for the addition or removal of tenants at runtime, facilitating scalability and adaptability to changing business needs.

- **Shared Infrastructure:** While tenants share the same application infrastructure, their data and processes remain segregated, optimizing resource usage without compromising security.

## Implementing Multitenancy in Cronus

1. **Tenant Identification:** Assign a unique identifier to each tenant to distinguish their data and operations within the system.

2. **Data Segregation:** Utilize strategies such as separate databases, schemas, or tables with tenant-specific identifiers to ensure data isolation.

3. **Configuration Management:** Maintain tenant-specific configurations to cater to individual requirements and preferences.

4. **Access Control:** Implement robust authentication and authorization mechanisms to enforce tenant boundaries and prevent cross-tenant data access.

## Best Practices

- **Consistent Tenant Context:** Ensure that the tenant context is consistently applied throughout the application to maintain data integrity and security.

- **Scalability Planning:** Design the system to handle varying numbers of tenants, considering factors like data volume, performance, and resource allocation.

- **Monitoring and Auditing:** Implement monitoring and auditing tools to track tenant-specific activities, aiding in compliance and troubleshooting.

By adhering to these practices, developers can leverage Cronus's multitenancy capabilities to build scalable, secure, and efficient applications that serve multiple clients effectively.

Cronus treats the **tenant** as a first-class dimension of every message. One running service can host many tenants side-by-side; their event streams, projections, message routing, and per-tenant services are all isolated by a single string.

## What a tenant is in Cronus

A tenant in Cronus is a short alphanumeric string (for example `acme`, `contoso`, `northwind`) that scopes everything the host does while processing a message:

* the keyspace / table prefix used by the event store,
* the keyspace / table prefix used by the projection store,
* the RabbitMQ exchange and queue names,
* any service you register with a per-tenant lifetime.

The tenant is not a property you carry around manually. Cronus takes the inbound message, resolves the tenant from it once, pins that tenant on the current async scope, and every component downstream that asks "which tenant am I in?" gets the same answer.

## Configuring the tenant list

The set of tenants a host serves is controlled by the `Cronus:Tenants` configuration key. See [`configuration.md`](../configuration.md) for the full schema.

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "boundedcontext": "billing",
    "tenants": [ "acme", "contoso" ]
  }
}
```
{% endcode %}

Each entry is lower-cased, trimmed, and validated by `TenantsOptions` against the regex `^\b([\w\d_]+$)`, so tenant names may only contain alphanumeric characters and underscores. The binding is performed by `TenantsOptionsProvider` under the setting key `cronus:tenants`.

{% code title="TenantsOptions.cs" %}
```csharp
public class TenantsOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "The configuration `Cronus:Tenants` is required.")]
    [CollectionRegularExpression(@"^\b([\w\d_]+$)")]
    public IEnumerable<string> Tenants { get; set; }
}
```
{% endcode %}

Inject `IOptionsMonitor<TenantsOptions>` anywhere you need to enumerate the configured tenants at runtime.

## How the tenant travels

While handling a message, Cronus creates an `IServiceScope` and attaches a `CronusContext` to it. The context holds the tenant and the scope's service provider:

{% code title="CronusContext.cs" %}
```csharp
public sealed class CronusContext
{
    public CronusContext(string tenant, IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(tenant))
            throw new ArgumentException(
                "Unknown tenant. CronusContext is not properly built. " +
                "Make sure that you have properly configured `cronus:tenants`.");
        if (serviceProvider is null) throw new ArgumentNullException(nameof(serviceProvider));

        Tenant = tenant;
        ServiceProvider = serviceProvider;
        Trace = new Dictionary<string, object>();
    }

    public string Tenant { get; private set; }
    public IServiceProvider ServiceProvider { get; private set; }
    public Dictionary<string, object> Trace { get; }
    public bool IsNotInitialized => string.IsNullOrEmpty(Tenant) || ServiceProvider is null;
    public bool IsInitialized => IsNotInitialized == false;
}
```
{% endcode %}

The context is exposed via `ICronusContextAccessor`, which stores it in an `AsyncLocal<CronusContextHolder>` so it flows across `await` boundaries without you passing it around. The `IsNotInitialized` short-circuit exists so callers outside a message-handling scope can detect that there is no current tenant rather than accidentally reading `null`.

## The tenant-resolver chain

Cronus resolves the tenant from the inbound message via a two-layer chain.

* A non-generic **dispatcher**, `TenantResolver : ITenantResolver`, receives the raw `object` and looks up a typed resolver for its runtime type. It also implements the single-tenant fallback: if no resolver returns a value but `Cronus:Tenants` has exactly one entry, that entry is used.
* Typed resolvers implement `ITenantResolver<T>`. `DefaultTenantResolver` ships implementations for `AggregateRootId`, `AggregateCommit`, `IMessage`, `IBlobId`, `CronusMessage`, and `string`. For `IMessage`, it first looks for a `Tenant` property, then falls back to scanning properties of type `IBlobId` and parsing their URN.

{% code title="TenantResolver.Resolve" %}
```csharp
string tenant = resolverCache.GetTenantFrom(source);

if (string.IsNullOrEmpty(tenant) == false)
    return tenant;

if (tenants.Tenants.Count() == 1)
    return tenants.Tenants.Single();

throw new UnableToResolveTenantException("Unable to resolve tenant.");
```
{% endcode %}

Registration is automated by `MultitenancyDiscovery`: every non-abstract type implementing `ITenantResolver<T>` found in the loaded assemblies is registered as a singleton for each `ITenantResolver<T>` interface it closes. To add your own resolver (for example one that pulls the tenant from a custom transport header), just implement `ITenantResolver<YourSource>` and let discovery pick it up.

## Per-tenant services

Two helpers make tenant-scoped singletons painless:

* `SingletonPerTenant<T>` resolves, caches, and returns a `T` per tenant, setting `IHaveTenant.Tenant` on the instance if the type implements that marker.
* `SingletonPerTenantContainer<T>` is the shared dictionary behind `SingletonPerTenant<T>` and disposes cached instances when the host shuts down.

Register them with `AddTenantSingleton<TService, TImplementation>` from [`CronusServiceCollectionExtensions`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/CronusServiceCollectionExtensions.cs):

{% code title="CronusServiceCollectionExtensions.cs (extract)" %}
```csharp
public static IServiceCollection AddTenantSingleton<TService, TImplementation>(this IServiceCollection services)
    where TService : class
    where TImplementation : class, TService
{
    services.AddTransient<TImplementation>();
    services.AddTransient<TService>(provider =>
        provider.GetRequiredService<SingletonPerTenant<TImplementation>>().Get());

    return services;
}
```
{% endcode %}

`AddTenantSupport` wires `SingletonPerTenant<>` as transient and `SingletonPerTenantContainer<>` as singleton, so every resolution of `TService` transparently dispatches to the correct tenant-specific instance.

## Aggregate ids carry the tenant

Every `AggregateRootId` encodes the tenant in its URN. The constructor takes the tenant first:

```csharp
public AggregateRootId(string tenant, string arName, string id)
```

See [`ids.md`](ids.md) for the full URN layout. `DefaultTenantResolver.Resolve(AggregateRootId id)` simply returns `id.Tenant`; this is why messages that expose an `AggregateRootId` property do not need to carry a separate `Tenant` string.

## End-to-end example

```csharp
// 1. Boot Cronus with two tenants.
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) => services.AddCronus(ctx.Configuration));

// 2. Register a tenant-scoped service.
builder.ConfigureServices(s => s.AddTenantSingleton<IRateLimiter, MemoryRateLimiter>());

// 3. Inside any handler, read the current tenant from the context.
public class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    private readonly ICronusContextAccessor ctx;
    public OrderCreatedHandler(ICronusContextAccessor ctx) => this.ctx = ctx;

    public Task HandleAsync(OrderCreated @event)
    {
        var tenant = ctx.CronusContext.Tenant; // "acme" or "contoso"
        // ...
        return Task.CompletedTask;
    }
}
```

## Guidelines

{% hint style="success" %}
You **can** read `ICronusContextAccessor.CronusContext.Tenant` anywhere inside a message-handling scope.

You **should** prefer `AddTenantSingleton<TService, TImpl>` over manually caching per-tenant state.

You **must** register every tenant in `Cronus:Tenants` before sending messages for it — `DefaultCronusContextFactory` rejects unknown tenants at scope creation time.
{% endhint %}

{% hint style="warning" %}
You **should not** put the tenant in the URL or a request body as a plain string and trust it. Resolve it from an authenticated claim or the aggregate id instead — that is what `ITenantResolver<T>` is for.
{% endhint %}
