# Domain Modeling

Cronus is built to let you express Domain-Driven Design tactical patterns with minimal friction. Aggregates, entities, value objects, commands, events, and signals all have first-class primitives and serialize/route through a predictable, conventions-first pipeline. If you are new to the DDD vocabulary, start with [`concepts/ddd.md`](../concepts/ddd.md); if you are new to event sourcing, read [`concepts/es.md`](../concepts/es.md).

The pages below cover the tactical building blocks you assemble into a Cronus-powered service.

| Page | What it covers |
|---|---|
| [Aggregate](aggregate.md) | `AggregateRoot<TState>` and `AggregateRootState<TAggregate, TId>` — the unit of consistency. |
| [Entity](entity.md) | `Entity<TAggregate, TState>` — identified objects that live inside an aggregate boundary. |
| [Value Object](value-object.md) | Immutable, equality-by-value types — Cronus does not ship a base class; use `record` or hand-written equality. |
| [IDs](ids.md) | `Urn`, `AggregateRootId`, `EntityId<TAggregateRootId>` — stable identity on the wire. |
| [Bounded Context](bounded-context.md) | The `Cronus:BoundedContext` setting and the `DataContract.Namespace` convention. |
| [Published Language](published-language.md) | `[DataContract(Name = "<guid>")]` and how GUIDs decouple serialization from class names. |
| [Multitenancy](multitenancy.md) | `CronusContext.Tenant`, `TenantsOptions`, and the resolver chain. |
| [Signals](signals.md) | `ISignal`, `ISignalHandle<T>`, and `ITrigger` — ambient pub/sub broadcasts. |
| [Messages](messages/) | Commands, events, public events, signals — the four lanes of the message bus. |
| [Handlers](handlers/) | Application services, gateways, ports, projections, sagas, triggers. |

Each page is self-contained and cross-references the others where a single concept spans multiple files.
