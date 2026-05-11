# AGENTS.md

Operating rules for AI coding agents (Claude Code, Cursor, Aider, Copilot Chat, Codex CLI) working in any project that consumes [Elders.Cronus](https://github.com/Elders/Cronus). This file curates facts that already live in the canonical [`docs/`](docs/) tree and the Cronus knowledge base — read it first, follow the pointers when you need depth.

## What Cronus is

Cronus is a .NET DDD/CQRS/Event Sourcing framework maintained by Elders OSS. It targets `net8.0;net9.0` (see `src/Elders.Cronus/Elders.Cronus.csproj`). Domain code is organised around aggregates that emit events, application services that handle commands, and projections / sagas / ports / triggers / gateways that react to events through RabbitMQ-backed transport with Cassandra-backed event store and projections.

## Hard rules — the things that will break the build or mislead you

These are not style preferences. Violating one of them either fails to compile, fails at runtime, or silently corrupts the wire contract.

1. **All publishing is async.** `IPublisher<T>` exposes `PublishAsync(...)` only. There is no sync `Publish(...)`. The lint pattern `sync-publish` flags any `publisher.Publish(` call.

   ```csharp
   // RIGHT
   await publisher.PublishAsync(command);

   // WRONG — does not exist
   publisher.Publish(command);
   ```

2. **Every `IMessage` needs a stable contract id and ordered members.** Every `ICommand`, `IEvent`, `IPublicEvent`, `ISignal` (and every projection state, value record persisted in a snapshot, etc.) must carry `[DataContract(Namespace = BC.Name, Name = "<fresh GUID>")]` plus `[DataMember(Order = N)]` on every persisted property. The GUID is the wire identity — once a message is in production, **never** change it.

   ```csharp
   [DataContract(Name = "728fc4e7-628b-4962-bd68-97c98aa05694")]
   public class TaskCreated : IEvent
   {
       TaskCreated() { }

       public TaskCreated(TaskId id, string name, DateTimeOffset timestamp)
       {
           Id = id; Name = name; Timestamp = timestamp;
       }

       [DataMember(Order = 1)] public TaskId Id { get; private set; }
       [DataMember(Order = 2)] public string Name { get; private set; }
       [DataMember(Order = 3)] public DateTimeOffset Timestamp { get; private set; }
   }
   ```

3. **Aggregate IDs use the non-generic `AggregateRootId(string tenant, string arName, string id)`.** The constructor order is `(tenant, arName, id)`. The generic `AggregateRootId<T>` form is commented out in source and is not available. Do not invent `AggregateUrn`, `IUrn`, or `StringTenantId` — those are removed types.

   ```csharp
   [DataContract(Name = "d5e50e1f-5886-4608-9361-9fe0eb440a6b")]
   public class TaskId : AggregateRootId
   {
       TaskId() { }
       public TaskId(string tenant, string id) : base(tenant, "task", id) { }
   }
   ```

4. **Aggregate roots inherit `AggregateRoot<TState>` and mutate state only through events.** Keep a `private` parameterless constructor for replay, validate invariants in public methods, and call `Apply(new SomeEvent(...))` to record changes. State is folded by `public void When(TEvent e)` handlers on the `AggregateRootState<TRoot, TRootId>` class. The aggregate must remain **synchronous** — no I/O, no `async`. See [`docs/cronus-framework/domain-modeling/aggregate.md`](docs/cronus-framework/domain-modeling/aggregate.md).

5. **Cross-aggregate flow goes through a Saga (process manager).** Aggregates do not subscribe to other aggregates' events, do not call other aggregates, and do not load anything via the repository. If you need to react to one aggregate's event by issuing a command on another, that is a saga's job. See [`docs/cronus-framework/domain-modeling/handlers/sagas.md`](docs/cronus-framework/domain-modeling/handlers/sagas.md).

## Pick the right handler

Six handler kinds. Pick by **intent**, not by capability. Verbatim from [`docs/cronus-framework/domain-modeling/handlers/README.md`](docs/cronus-framework/domain-modeling/handlers/README.md):

| Handler | Reacts to | Produces | Side effects allowed? |
| --- | --- | --- | --- |
| Application Service | `ICommand` | New events on a single aggregate | No — should not perform I/O outside the event store |
| Projection | `IEvent` | A read model (snapshot or external store) | No — must not publish commands or events |
| Saga | `IEvent`, `IScheduledMessage` | New `ICommand` messages; scheduled timeouts | No business-facing side effects — coordinate aggregates |
| Port | `IEvent` | New `ICommand` messages | Yes — the classic "send email", "call external API" place |
| Trigger | `IEvent`, `ISignal` | Anything — typically starts a job or a downstream workflow | Yes |
| Gateway | `IEvent` | New `ICommand` messages, with tracked infrastructure state | Yes — owns metadata required by an external system |

Rule of thumb:

- One command mutates one aggregate → **Application Service**.
- Read model from events → **Projection**.
- Coordinate several aggregates → **Saga**.
- React to an event with one outbound side effect (email, HTTP call) → **Port**.
- Kick off a job or long-running workflow → **Trigger**.
- Port that needs persistent infra state (push tokens, badges) → **Gateway**.

## Pick the right message type

Every message implements `IMessage` and carries a `Timestamp`. Source: [`docs/cronus-framework/domain-modeling/messages/README.md`](docs/cronus-framework/domain-modeling/messages/README.md).

| Message type | Intent | Consumed by |
| --- | --- | --- |
| `ICommand` | Request a business change. May be rejected by the aggregate. Imperative name (`CreateTask`). | Application Service |
| `IEvent` | Record a fact already committed inside this bounded context. Past-tense name (`TaskCreated`). | Projection, Saga, Port, Trigger, Gateway |
| `IPublicEvent` | Announce a change to the outside world (published language). Carries the originating `Tenant`. | Subscribers in other bounded contexts |
| `ISignal` | Trigger arbitrary side-effects (heartbeats, rebuilds, process pings). | Trigger |

Publishing is always async:

```csharp
Task<bool> PublishAsync(TMessage message, Dictionary<string, string> headers = null);
Task<bool> PublishAsync(TMessage message, DateTime publishAt, Dictionary<string, string> headers = null);
Task<bool> PublishAsync(TMessage message, TimeSpan publishAfter, Dictionary<string, string> headers = null);
```

`false` from `PublishAsync` means the transport rejected the message — surface it.

## Banned legacy types

These names exist in old samples, blog posts, and Cronus v6/v7 code. They are gone in current Cronus and the lint database flags every one of them. Never propose or generate them:

- **`ValueObject<T>`** — base class is removed. Use C# `record` types with `[DataContract(Name = "<guid>")]`. KB concept: `value-object-record-pattern`.
- **`IUrn`**, **`AggregateUrn`** — removed; use `AggregateRootId` and `AggregateRootId.TryParse`.
- **`StringTenantId`** — removed; tenant is a `string` segment of `AggregateRootId`.
- **`AggregateRootId<T>`** (generic form) — commented out in source. Use the non-generic `AggregateRootId(tenant, arName, id)`.
- **`IAggregateRootId<T>`** — removed; the lint pattern is `fake-iaggregaterootid-generic`.
- **`AggregateRootApplicationService<T>`** — old base class. Use `ApplicationService<TAggregate>`.
- **`Cronus.Persistence.Git-*`**, **`Cronus.Persistence.MSSQL`**, **`Cronus.Serialization.Proteus`** — legacy persistence/serialization satellites. Don't add them. The current stack is `Cronus.Persistence.Cassandra`, `Cronus.Projections.Cassandra`, `Cronus.Transport.RabbitMQ`, `Cronus.Serialization.NewtonsoftJson`.
- **Sync `IPublisher.Publish(...)`** — removed; use `PublishAsync`.
- **Sync `IProjectionReader.Get(...)`** — removed; use the async `GetAsync`.
- **Sync `repository.Save(...)`** — removed; use `SaveAsync`.
- **`repository.TryLoad<T>(id, out var ar)`** — removed; use `await repository.LoadAsync<T>(id)` and inspect the `ReadResult<AR>` (`IsSuccess`, `NotFound`, `HasError`).
- **`public void Handle(TMessage m)` on a handler** — removed; handlers are `Task HandleAsync(TMessage m)`.

If the user asks for one of these, tell them it has been removed and offer the modern equivalent.

## `AddCronus(configuration)` setup

`services.AddCronus(configuration)` is the single entry point — it registers core services, scans your assemblies for handlers, and binds options. The required configuration keys are:

- `Cronus:BoundedContext` — alphanumeric/underscore name of the service. Validates against `^\b([\w\d_]+$)`.
- `Cronus:Tenants` — non-empty string array; same character set per element.
- `Cronus:Persistence:Cassandra:ConnectionString` — Cassandra event-store connection.
- `Cronus:Projections:Cassandra:ConnectionString` — Cassandra projections-store connection.
- `Cronus:Transport:RabbitMQ:*` — Server, Port, VHost, Username, Password.

Every key, type and default lives in [`docs/cronus-framework/configuration.md`](docs/cronus-framework/configuration.md). When in doubt, read it — do not invent option names.

Common process-split flags (default `true`):

- `Cronus:ApplicationServicesEnabled`
- `Cronus:ProjectionsEnabled`
- `Cronus:SagasEnabled`
- `Cronus:PortsEnabled`
- `Cronus:GatewaysEnabled`
- `Cronus:TriggersEnabled`

For an API process that only publishes commands, turn all six off and let a separate worker host run them.

For local dev with [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/), the AppHost spins up Cassandra/RabbitMQ/Redis/Consul/Elasticsearch and injects connection details as `Cronus__*` environment variables that `AddCronus` picks up automatically — see KB concept `aspire-cronus-wiring`.

## `Bootstraps` enum and `[CronusStartup]`

One-time host startup work goes in `ICronusStartup` / `ICronusTenantStartup` and is ordered by `[CronusStartup(Bootstraps.X)]`:

| Phase | Value | Use it for |
| --- | ---: | --- |
| `Environment` | `0` | Process-wide switches, logger setup |
| `ExternalResource` | `10` | Provision DB keyspaces, broker exchanges |
| `Configuration` | `20` | Finalise options |
| `Aggregates` | `30` | One-time work for aggregates |
| `Ports` | `40` | One-time work for ports |
| `Sagas` | `50` | One-time work for sagas |
| `EventStoreIndices` | `55` | Register per-tenant event-store indices |
| `Projections` | `60` | One-time work for projections |
| `Gateways` | `70` | One-time work for gateways |
| `Runtime` | `1000` | Default — anything else |

Pick the smallest phase number that satisfies your ordering. Startups must be safe to run repeatedly. The attribute does **nothing** on a discovery — do not put it there. Source: [`docs/cronus-framework/extensibility/startup-attribute.md`](docs/cronus-framework/extensibility/startup-attribute.md).

## Knowledge base for grounded answers

A curated KB of Cronus symbols, doc snippets, lint rules and concepts lives at `E:/Projects/ai stuff/Cronus-AIed/`. Use the CLI before generating non-trivial Cronus code:

```bash
CRONUS_KB_TRACE=1 python "E:/Projects/ai stuff/Cronus-AIed/benchmark/scripts/knowledge-lookup.py" \
    {concepts,symbols,docs,snippets,lint} ...
```

Always set `CRONUS_KB_TRACE=1` so usage telemetry accumulates.

The 15 curated concepts (run `concepts` with no args to list them) include `aspire-cronus-wiring`, `tenant-flow`, `multi-process-topology`, `discoveries-pattern`, `port-external-io`, `saga-external-service`, `trigger-signal-rpc`, `public-event-federation`, `aggregate-with-entities`, `value-object-record-pattern`, `contract-versioning`, `dual-store-projections`, `projection-versioning`, `atomic-action-locking`, `approval-workflow`.

Before writing a saga / projection / port / etc., ground the shape:

```bash
CRONUS_KB_TRACE=1 python ".../knowledge-lookup.py" concepts --id <best-guess> --expand
```

Other useful sub-commands:

- `symbols --kind aggregate-root --canonical` — production-quality real symbols of a given kind.
- `docs --topic "<text>"` — scope to specific docs.
- `lint --file <path>` — run the lint patterns against a file you just wrote.
- `lint --list` — see the patterns (the `legacy` patterns map to the **Banned legacy types** section above).

## Verification before claiming done

Cronus tasks are not done until both of these are green from a clean checkout:

```shell
dotnet build
dotnet test
```

Lint your output too:

```shell
CRONUS_KB_TRACE=1 python ".../knowledge-lookup.py" lint --file path/to/your/file.cs
```

If the lint reports any `legacy` or `bug` finding, fix it before declaring success. "I think this works" is not a status.

## Pointers

Canonical doc pages on this branch:

- [Bounded context](docs/cronus-framework/domain-modeling/bounded-context.md)
- [Aggregate](docs/cronus-framework/domain-modeling/aggregate.md)
- [Aggregate IDs](docs/cronus-framework/domain-modeling/ids.md)
- [Messages overview](docs/cronus-framework/domain-modeling/messages/README.md) — commands, events, public events, signals
- [Handlers overview](docs/cronus-framework/domain-modeling/handlers/README.md) — application services, projections, sagas, ports, triggers, gateways
- [Configuration](docs/cronus-framework/configuration.md) — every `Cronus:*` key
- [Discoveries](docs/cronus-framework/extensibility/discoveries.md) and [`[CronusStartup]`](docs/cronus-framework/extensibility/startup-attribute.md)
- [Workflows](docs/cronus-framework/workflows.md) — message-processing pipeline
- [Indices](docs/cronus-framework/indices.md) — event-store secondary indices
- [Quick start: setup](docs/getting-started/quick-start/setup.md) and [persist first event](docs/getting-started/quick-start/persist-first-event.md)
