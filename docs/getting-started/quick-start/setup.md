---
description: Prepare a two-process TaskManager skeleton — API + worker — with Cronus, Cassandra and RabbitMQ.
---

# Setup

This quick start builds a tiny task-management service in two processes:

* **TaskManager.Api** — an ASP.NET Core Web API that accepts requests from clients and publishes commands to Cronus.
* **TaskManager.Service** — a worker host that consumes commands, persists events, builds projections, and runs the rest of the Cronus pipeline.

Splitting API and worker is the recommended production topology. The API process stays fast and stateless; the worker process owns all long-running background work.

## Prerequisites

* .NET 8 or .NET 9 SDK
* Docker (for Cassandra and RabbitMQ)
* An IDE — Visual Studio, Rider, or VS Code

## 1. Create the solution

```shell
mkdir TaskManager && cd TaskManager
dotnet new sln --name TaskManager

dotnet new webapi --name TaskManager.Api
dotnet new worker --name TaskManager.Service

dotnet sln add TaskManager.Api TaskManager.Service
```

## 2. Add the Cronus packages

The API only publishes commands, so it needs the core package; the worker persists events, builds projections and moves data over RabbitMQ, so it needs the full transport and persistence story.

```shell
# API — publisher only
dotnet add TaskManager.Api package Cronus

# Worker — full Cronus host
dotnet add TaskManager.Service package Cronus
dotnet add TaskManager.Service package Cronus.Transport.RabbitMQ
dotnet add TaskManager.Service package Cronus.Persistence.Cassandra
dotnet add TaskManager.Service package Cronus.Projections.Cassandra
dotnet add TaskManager.Service package Cronus.Serialization.NewtonsoftJson
```

{% hint style="warning" %}
The projections package is `Cronus.Projections.Cassandra` (plural). The older, singular `Cronus.Projection.Cassandra` is a different and obsolete package name.
{% endhint %}

## 3. Start the backing services

From a clean Docker environment, the simplest way to get Cassandra and RabbitMQ running is:

```shell
# Cassandra 4.0 — Cronus EventStore
docker run --restart=always -d --name cassandra \
    -p 9042:9042 \
    cassandra:4.0

# RabbitMQ 3.9.11 (eldersoss image — management plugin included)
docker run --restart=always -d --name rabbitmq \
    -p 5672:5672 -p 15672:15672 \
    -e RABBITMQ_DEFAULT_USER=user \
    -e RABBITMQ_DEFAULT_PASS=pass \
    -e RABBITMQ_DEFAULT_VHOST=rabbit \
    eldersoss/rabbitmq:3.9.11
```

The image tags above match the ones used by the Elders platform's shared `docker-compose.yml`. Use them to keep local development aligned with CI/CD.

{% hint style="info" %}
The RabbitMQ management UI is at [http://localhost:15672](http://localhost:15672) (user `user` / pass `pass`). Cassandra has no bundled UI — use [DataStax DevCenter](https://downloads.datastax.com/#devcenter) or `cqlsh` to inspect tables.
{% endhint %}

## 4. Write `appsettings.json`

Both processes share the same Cronus configuration. Create identical `appsettings.json` files in `TaskManager.Api` and `TaskManager.Service`:

//This should be int the Service and in the Api.

{% code title="appsettings.json" %}
```json
{
  "Cronus": {
    "BoundedContext": "taskmanager",
    "Tenants": [ "tenant" ],
    "Transport": {
      "RabbitMQ": {
        "Server": "127.0.0.1",
        "Port": 5672,
        "VHost": "rabbit",
        "Username": "user",
        "Password": "pass"
      }
    },
    "Persistence": {
      "Cassandra": {
        "ConnectionString": "Contact Points=127.0.0.1;Port=9042;Default Keyspace=taskmanager_es"
      }
    },
    "Projections": {
      "Cassandra": {
        "ConnectionString": "Contact Points=127.0.0.1;Port=9042;Default Keyspace=taskmanager_projections"
      }
    }
}
}
```
{% endcode %}

The full list of configuration keys lives on the configuration page:

{% content-ref url="../../cronus-framework/configuration.md" %}
[configuration.md](../../cronus-framework/configuration.md)
{% endcontent-ref %}

## 5. Turn off background consumers in the API

The API process must not run application-service or projection consumers — that is the worker's job. Switch the relevant feature flags off in `TaskManager.Api/appsettings.json`:

{% code title="TaskManager.Api/appsettings.json" %}
```json
{
  "Cronus": {
    "ApplicationServicesEnabled": false,
    "ProjectionsEnabled": false,
    "PortsEnabled": false,
    "SagasEnabled": false,
    "GatewaysEnabled": false,
    "TriggersEnabled": false
  }
}
```
{% endcode %}

{% hint style="success" %}
You **should** turn off every consumer in the API process. The API only needs the publisher side of Cronus — it pushes commands to the bus and returns. The worker process keeps the defaults (all consumers `true`) and does the heavy lifting.
{% endhint %}

## 6. Wire the worker

{% code title="TaskManager.Service/Program.cs" %}
```csharp
using Elders.Cronus;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddCronus(ctx.Configuration);
        services.AddHostedService<CronusBackgroundService>();
    })
    .Build();

await host.RunAsync();

sealed class CronusBackgroundService : BackgroundService
{
    private readonly ICronusHost cronus;
    public CronusBackgroundService(ICronusHost cronus) => this.cronus = cronus;

    protected override async Task ExecuteAsync(CancellationToken _)
    {
        await cronus.StartAsync().ConfigureAwait(false);
    }
}
```
{% endcode %}

`services.AddCronus(configuration)` registers every Cronus core service, scans your assemblies for application services, projections, sagas, ports, triggers and gateways, and binds the configuration options. `ICronusHost.StartAsync()` starts the subscribers based on your feature flags.

## 7. Wire the API

{% code title="TaskManager.Api/Program.cs" %}
```csharp
using Elders.Cronus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCronus(builder.Configuration);

var app = builder.Build();

app.MapControllers();
await app.RunAsync();
```
{% endcode %}

The API can now inject `IPublisher<ICommand>` into controllers and dispatch commands.

## 8. Run both processes

Open two terminals:

```shell
# Terminal 1 — worker
dotnet run --project TaskManager.Service

# Terminal 2 — API
dotnet run --project TaskManager.Api
```

If both processes start cleanly (no exceptions in the logs) your Cronus skeleton is ready. Proceed to the next page to model a domain and persist your first event.

{% content-ref url="persist-first-event.md" %}
[persist-first-event.md](persist-first-event.md)
{% endcontent-ref %}
