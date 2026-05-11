# Aspire and Cronus

[.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) is the canonical way to compose a Cronus solution today. The AppHost owns the topology — Cassandra, RabbitMQ, Redis, Consul, Elasticsearch, plus every Cronus process that consumes them — and it injects connection strings into each process as environment variables that map directly onto the `Cronus:*` configuration keys. The Cronus side stays standard ASP.NET Core hosting; the only thing the AppHost replaces is "how does the worker find the broker?".

This page is the wiring contract: what the AppHost must do, what the worker must do, and how the two halves talk through `IConfiguration`.

## What the AppHost is responsible for

The AppHost is a [`DistributedApplication`](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host-overview) project. It does three things for Cronus:

1. **Stand up infrastructure resources** — Cassandra, RabbitMQ, Redis, optionally Consul, Elasticsearch — using the standard Aspire builder API (`builder.AddRedis`, `builder.AddRabbitMQ`, `builder.AddContainer` for things without a first-party integration).
2. **Reference each resource from each Cronus process** with `WithReference(...)`, so Aspire knows the dependency graph and provides the connection strings.
3. **Map every connection string onto the `Cronus:*` configuration key the worker expects**, using `WithEnvironment(...)` and the double-underscore convention that ASP.NET Core configuration interprets as `Cronus:Foo:Bar`.

The third step is the only Cronus-specific bit. The worker process expects (for example) `Cronus:Persistence:Cassandra:ConnectionString`. Aspire only knows it has a Cassandra container at some endpoint. The AppHost is the place where you say "the contact points the worker should use are this Aspire endpoint":

```csharp
.WithEnvironment("Cronus__Persistence__Cassandra__ConnectionString",
    () => $"Contact Points={cassandra.GetEndpoint("cql").Host};Port={cassandra.GetEndpoint("cql").Port};Default Keyspace=billing")
```

Pulled from the Locus AppHost — see [`Elders.Locus.AppHost/AppHost.cs`](https://github.com/Elders/locus.backend/blob/master/src/Elders.Locus.AppHost/AppHost.cs) for the whole file.

## What the worker is responsible for

The worker stays a vanilla ASP.NET Core or worker-service host. It calls `AddServiceDefaults()` (the Aspire defaults extension method), `AddCronus(configuration)`, and that is the wiring done. Nothing in the Cronus call chain knows about Aspire — the configuration providers built into `IConfiguration` already turn `Cronus__Persistence__Cassandra__ConnectionString` (env var) into `Cronus:Persistence:Cassandra:ConnectionString` (configuration key), and `AddCronus` reads from there.

The Locus API's `Program.cs` shows the canonical ordering — service defaults first, then `AddCronus`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// ...

builder.Services.AddCronusAspNetCore();
builder.Services.AddCronus(configuration);
```

Source: [`Elders.Locus.Api/Program.cs`](https://github.com/Elders/locus.backend/blob/master/src/Elders.Locus.Api/Program.cs).

[`AddCronus`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/CronusServiceCollectionExtensions.cs) takes the `IConfiguration` from the host, runs the discovery scan ([`DiscoveryScanner`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Discoveries/DiscoveryScanner.cs)) over every assembly that `AssemblyLoader` found in the output directory, and registers everything the satellites declare. There is no Aspire-specific call.

## A minimal AppHost

A worker host that needs Cassandra, RabbitMQ and Redis looks like this:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

var cassandra = builder.AddContainer("cassandra", "cassandra", "latest")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint(port: 9042, targetPort: 9042, name: "cql")
    .WithEnvironment("CASSANDRA_CLUSTER_NAME", "BillingCluster");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin(15672);

builder.AddProject<Projects.Billing_Worker>("billing-worker")
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithEnvironment("Cronus__Persistence__Cassandra__ConnectionString",
        () => $"Contact Points={cassandra.GetEndpoint("cql").Host};Port={cassandra.GetEndpoint("cql").Port};Default Keyspace=billing")
    .WithEnvironment("Cronus__Projections__Cassandra__ConnectionString",
        () => $"Contact Points={cassandra.GetEndpoint("cql").Host};Port={cassandra.GetEndpoint("cql").Port};Default Keyspace=billing_projections")
    .WithEnvironment("Cronus__Transport__RabbitMQ__Server",
        () => rabbitmq.GetEndpoint("tcp").Host)
    .WithEnvironment("Cronus__Transport__RabbitMQ__Port",
        () => rabbitmq.GetEndpoint("tcp").Port.ToString())
    .WithEnvironment("Cronus__Transport__RabbitMQ__Username", rabbitmq.Resource.UserNameReference)
    .WithEnvironment("Cronus__Transport__RabbitMQ__Password", rabbitmq.Resource.PasswordParameter)
    .WithEnvironment("Cronus__AtomicAction__Redis__ConnectionString",
        () => $"{redis.GetEndpoint("tcp").Host}:{redis.GetEndpoint("tcp").Port}")
    .WaitFor(redis)
    .WaitFor(cassandra)
    .WaitFor(rabbitmq);

builder.Build().Run();
```

The pattern repeats for every Cronus process the AppHost owns.

## Configuration mapping cheat sheet

| Aspire resource | Maps to Cronus key |
| --- | --- |
| `cassandra.GetEndpoint("cql")` | [`Cronus:Persistence:Cassandra:ConnectionString`](configuration.md#cronus-persistence-cassandra-connectionstring), [`Cronus:Projections:Cassandra:ConnectionString`](configuration.md#cronus-projections-cassandra-connectionstring) |
| `rabbitmq.GetEndpoint("tcp")` | [`Cronus:Transport:RabbitMQ:Server`](configuration.md#cronus-transport-rabbitmq-server), [`Cronus:Transport:RabbitMQ:Port`](configuration.md#cronus-transport-rabbitmq-port) |
| `rabbitmq.GetEndpoint("management")` | [`Cronus:Transport:RabbitMQ:AdminPort`](configuration.md#cronus-transport-rabbitmq-adminport) |
| `redis.GetEndpoint("tcp")` | [`Cronus:AtomicAction:Redis:ConnectionString`](configuration.md#cronus-atomicaction-redis-connectionstring) |
| `consul.GetEndpoint("http")` | `Cronus:Cluster:Consul:Address` (where used) |

For the full key reference see:

{% content-ref url="configuration.md" %}
[configuration.md](configuration.md)
{% endcontent-ref %}

## Multi-process topologies

The AppHost is also where you decide how many Cronus processes there are and what each one does. A typical Aspire-era Cronus solution has at least two — an API host that dispatches commands and a worker host that runs projections, sagas, ports and gateways. The split is controlled by `Cronus:*Enabled` flags injected per process. That story has its own page:

{% content-ref url="multi-process-topology.md" %}
[multi-process-topology.md](multi-process-topology.md)
{% endcontent-ref %}

## Best Practices

{% hint style="success" %}
**You can / should / must**

* the AppHost **must** map every required `Cronus:*` connection string with `WithEnvironment(...)` — Cronus does not auto-discover Aspire endpoints
* the AppHost **must** declare `WithReference(...)` for every infrastructure resource a process touches, so Aspire wires service discovery and credentials
* the AppHost **should** call `WaitFor(...)` on every infrastructure dependency so processes do not start before their broker is reachable
* the worker **should** call `AddServiceDefaults()` before `AddCronus(configuration)` so OpenTelemetry, health checks and resilience are in place when Cronus boots
{% endhint %}

{% hint style="warning" %}
**You should not**

* the AppHost **should not** use the double-colon form (`Cronus:Foo:Bar`) for environment variables — use the double-underscore form (`Cronus__Foo__Bar`) so Linux containers receive the value
* the worker **should not** call `AddCronus` more than once or before `IConfiguration` has all of its providers — the discovery scan runs eagerly during the call
* a process **should not** ship its own bootstrap of Cassandra/RabbitMQ/Redis next to the AppHost — let the AppHost own the resource lifetime
{% endhint %}
