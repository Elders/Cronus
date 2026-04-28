# Multi-process topology

A Cronus solution rarely runs as a single process in production. Once traffic grows, the workload splits: an API host has to answer HTTP fast, a worker host has to chew through events at its own pace, an offline scraper has to pour data in without holding up either, and a console may need to run a one-off bulk operation against the same event store. All four are the same Cronus solution — same bounded context, same configuration, same event store and broker — but each does only the part of the work it is meant to do.

The split is controlled by a small family of boolean flags. This page explains the pattern, the toggles, and the topologies that fall out of them.

## Why split

You split a Cronus host when one of these things stops being true:

* The HTTP-facing process is fast enough to keep the user waiting only for the command write, not for projections, sagas and gateways to settle.
* A long replay (a versioning rebuild, a migration, a backfill) does not steal capacity from production traffic.
* External-facing side effects — emails, push notifications, third-party calls — happen on a host that can be restarted, throttled or paused without taking the API down.
* Read-only workloads can scale horizontally without each replica trying to advance write-side state.

Each of those is a `Cronus:*Enabled` flag flipped on a different host.

## The toggle pattern

Cronus core ships nine flags, all on [`CronusHostOptions`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/CronusHostOptions.cs) and bound under `cronus:`:

| Toggle | Default | What it starts |
| --- | --- | --- |
| `Cronus:ApplicationServicesEnabled` | `true` | The application-services consumer (commands → aggregates) and the event-store indices consumer |
| `Cronus:ProjectionsEnabled` | `true` | The projections consumer (events → projection store) |
| `Cronus:PortsEnabled` | `true` | The ports consumer (events → outbound side effects with no persistent state) |
| `Cronus:SagasEnabled` | `true` | The sagas consumer (events → new commands and timeouts) |
| `Cronus:GatewaysEnabled` | `true` | The gateways consumer (events → external systems with persistent infrastructure state) |
| `Cronus:TriggersEnabled` | `true` | The triggers consumer (event hooks) |
| `Cronus:SystemServicesEnabled` | `true` | Cronus-internal system app services, sagas, ports, triggers, projections, system indices |
| `Cronus:MigrationsEnabled` | `false` | The replay/migration consumer |
| `Cronus:RpcApiEnabled` | `false` | The RPC API host (request/response over RabbitMQ) |

The flags are read by [`CronusHost`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/CronusHost.cs) at start. Each flag gates a `consumer.StartAsync()` call — if the flag is `false`, the consumer is built (the container still resolves it) but never started, so it never pulls from RabbitMQ. The flags are also re-evaluated when the options are reloaded, which means you can toggle a host between modes by changing configuration.

For the full reference of these keys see:

{% content-ref url="configuration.md" %}
[configuration.md](configuration.md)
{% endcontent-ref %}

Two consumers — `MigrationsEnabled` and `RpcApiEnabled` — default to `false`. Every other consumer defaults to `true`. The default Cronus host is therefore an "all-in-one" worker; you turn things off as you split it.

## Common topologies

### All-in-one (development, small services)

Every flag at default. One process does everything. Simple, fine until traffic forces you to split.

### API host + worker host (the canonical split)

Two processes share the same configuration except for the flags:

| Flag | API host | Worker host |
| --- | --- | --- |
| `ApplicationServicesEnabled` | `true` | `true` |
| `ProjectionsEnabled` | `false` | `true` |
| `PortsEnabled` | `false` | `true` |
| `SagasEnabled` | `false` | `true` |
| `GatewaysEnabled` | `false` | `true` |
| `TriggersEnabled` | `false` | `true` |
| `SystemServicesEnabled` | `true` | `true` |

The API host accepts commands over HTTP, dispatches them to application services and returns; nothing else runs in-process. The worker picks up the resulting events and turns them into projections, command-publishing sagas, side-effect ports and external-system gateways. Both processes call `IAggregateRepository.SaveAsync` (the API on every command, the worker through sagas), so both must have a real `IAggregateRootAtomicAction` configured — see [Atomic Actions](atomic-actions.md).

### Read-only host (scale-out reads)

Add a third process that does no writes at all:

| Flag | Read-only host |
| --- | --- |
| `ApplicationServicesEnabled` | `false` |
| `ProjectionsEnabled` | `false` |
| `PortsEnabled` | `false` |
| `SagasEnabled` | `false` |
| `GatewaysEnabled` | `false` |
| `TriggersEnabled` | `false` |
| `SystemServicesEnabled` | `false` |

It serves reads from the projection store and never advances any write-side state. You can run as many of these as the load needs, behind a load balancer.

### Migration host

A short-lived process that runs a replay or a backfill without disturbing production:

| Flag | Migration host |
| --- | --- |
| `ApplicationServicesEnabled` | `false` |
| `ProjectionsEnabled` | `false` |
| `MigrationsEnabled` | `true` |

Production hosts keep `MigrationsEnabled = false` so they never compete for the migration queue. When the replay finishes, the migration host shuts down.

### Scrapers and consoles

A scraper is a long-running process that pulls data from an external source and publishes commands; a console is a short one-off. Both typically run with most consumers off — they do not need to dispatch commands or run projections — and only the message-publishing infrastructure on. The Locus solution's [Scrapers](https://github.com/Elders/locus.backend/tree/master/src/Scrapers) and [`Elders.Locus.BulkOperations.Console`](https://github.com/Elders/locus.backend/tree/master/src/Elders.Locus.BulkOperations.Console) are the reference shape.

## Shared infrastructure

Every process in the topology sees the same:

* **Event store** ([`Cronus:Persistence:Cassandra:*`](configuration.md#cronus-persistence-cassandra)) — appends from any host go into the same keyspaces; loads from any host hit the same data.
* **Projection store** ([`Cronus:Projections:Cassandra:*`](configuration.md#cronus-projections-cassandra)) — the worker writes; everyone reads.
* **Transport** ([`Cronus:Transport:RabbitMQ:*`](configuration.md#cronus-transport-rabbitmq)) — the API publishes commands and events; the worker consumes them. Each consumer type has its own queue, so flipping a flag on one host does not starve another.
* **Atomic action** ([`Cronus:AtomicAction:Redis:*`](configuration.md#cronus-atomicaction-redis)) — every process that writes must reach the same Redis. See [Atomic Actions](atomic-actions.md).

The bounded-context name and tenant list — [`Cronus:BoundedContext`](configuration.md#cronus-boundedcontext) and [`Cronus:Tenants`](configuration.md#cronus-tenants) — must match across every process, because they are part of how queues, exchanges and keyspaces are named.

## Wiring the topology with Aspire

The natural place to define a topology is the AppHost. Each process is a `builder.AddProject<...>()` with its own `WithEnvironment("Cronus__ProjectionsEnabled", "false")` overrides, while sharing one `cassandra`, `rabbitmq` and `redis` resource. See:

{% content-ref url="aspire-cronus-wiring.md" %}
[aspire-cronus-wiring.md](aspire-cronus-wiring.md)
{% endcontent-ref %}

## Best Practices

{% hint style="success" %}
**You can / should / must**

* every process **must** share the same `Cronus:BoundedContext` and `Cronus:Tenants` — they identify the queues and keyspaces the topology lives in
* every writing process **must** be configured with a real atomic action; "writing" means anywhere `SaveAsync` is called, including sagas
* a topology **should** start as all-in-one and split only when a real bottleneck appears — the toggles are reversible, the architecture should be too
* a topology **can** mix Aspire-managed and externally-managed processes (a long-lived worker in Aspire, a CI-triggered console outside it) as long as both see the same configuration
{% endhint %}

{% hint style="warning" %}
**You should not**

* a topology **should not** turn off `SystemServicesEnabled` on a writing host — the system consumers are how versioning, indices and similar internal flows progress
* a topology **should not** run two processes with `MigrationsEnabled = true` against the same bounded context at the same time
* a topology **should not** depend on flag changes at runtime to fence off bad behaviour — disable the flag *and* stop the host if you really do not want it doing the work
{% endhint %}
