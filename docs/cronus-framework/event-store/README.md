# Event Store

The event store is the append-only log of domain events that backs every Cronus aggregate. It is the single source of truth for the write side of your service: everything your service can "remember" about what happened — every command outcome, every state change — is derived from the events that live in this log. Projections, indices and read models are all derived artefacts that can be thrown away and rebuilt from the event store.

## What the event store is

Every time an aggregate successfully handles a command it produces zero or more events. Cronus groups those events together with the aggregate root id, the new revision and a timestamp into an [`AggregateCommit`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/AggregateCommit.cs) and appends it to the event store. An aggregate is later rehydrated by loading all commits for its id and replaying the events through the `When` methods of its state.

The contract is deliberately small. It appends, loads, paginates and deletes — that is all:

```csharp
public interface IEventStore
{
    Task AppendAsync(AggregateCommit aggregateCommit);
    Task AppendAsync(AggregateEventRaw eventRaw);
    Task<EventStream> LoadAsync(IBlobId aggregateId);
    Task<bool> DeleteAsync(AggregateEventRaw eventRaw);
    Task<LoadAggregateRawEventsWithPagingResult> LoadWithPagingAsync(IBlobId aggregateId, PagingOptions pagingOptions);
    Task<AggregateEventRaw> LoadAggregateEventRaw(IndexRecord indexRecord);
}
```

## Core abstractions

The event-store subsystem exposes a handful of types that you will see repeatedly throughout the framework:

* [`IEventStore`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/IEventStore.cs) — append, load and delete aggregate commits. The canonical implementation you talk to from domain code is wrapped by [`CronusEventStore`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/CronusEventStore.cs), which adds structured logging on top of the backend store.
* [`IEventStorePlayer`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/IEventStorePlayer.cs) — enumerates every event in the store without caring about aggregate boundaries. Used by index and projection rebuilds. See [EventStore Player](eventstore-player.md).
* [`AggregateCommit`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/AggregateCommit.cs) — the on-the-wire unit of a write: `AggregateRootId`, `Revision`, list of private `IEvent`s, list of `IPublicEvent`s and a timestamp.
* [`EventStream`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/EventStream.cs) — the result of loading a single aggregate. It holds the ordered list of commits and exposes `TryRestoreFromHistory<T>(out T aggregateRoot)` to rebuild an aggregate instance.
* [`AggregateRepository`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/AggregateRepository.cs) — the repository you normally interact with from an application service. It delegates to `IEventStore` and handles integrity checks, atomic actions and duplicate-timestamp detection.

A write therefore looks like: application service loads the aggregate via `AggregateRepository.LoadAsync`, calls a domain method, which `Apply`s events, then `SaveAsync` wraps those events into an `AggregateCommit` and calls `IEventStore.AppendAsync`.

## Canonical backend: Cassandra

The production-grade event-store backend for Cronus is [`Cronus.Persistence.Cassandra`](https://github.com/Elders/Cronus.Persistence.Cassandra). It has been in production use since 2013 and is marked `olympus` in the [ecosystem](https://github.com/Elders/Cronus). Cassandra is a natural fit: the access pattern of an event store (append-only, partitioned by aggregate id, ordered by revision) maps cleanly to Cassandra's wide rows, and its distributed replication story covers the durability requirements without extra effort.

If you are wiring up a new service, use the Cassandra persister. The connection strings and replication settings are documented under `Cronus:Persistence:Cassandra:*` in [Configuration](../configuration.md#cronus-persistence-cassandra).

## Secondary backends

Cronus ships with a handful of alternative stores; their maturity and intended use are described in the [ecosystem](https://github.com/Elders/Cronus) reference:

* [`Cronus.Persistence.CosmosDb`](https://github.com/Elders/Cronus.Persistence.CosmosDb) — alternative cloud-native store.
* [`Cronus.Persistence.MSSQL`](https://github.com/Elders/Cronus.Persistence.MSSQL) — `styx`. Worked for Cronus v1, but a relational database is a poor match for an append-only log and we do not recommend it for new work.
* [`Cronus.Persistence.Git-`](https://github.com/Elders/Cronus.Persistence.Git-) — `tartarus`. Exists "just for fun".

## Schema evolution

The events you persist today will outlive your current code. When a business rule changes, you rename a field or split an event in two, the old events stay on disk forever — you do not get to "edit migrations". Cronus provides a set of migration primitives for those situations (copy an event store with transformations, delete events, validate a store after a migration). See [Migrations](migrations/README.md) and [Copy EventStore](migrations/copy-eventstore.md).

## Indices

Reading events by aggregate id is cheap. Reading events by type, or counting them, is not — the store is partitioned by aggregate, not by event type. Cronus maintains a secondary index subsystem to make those queries possible. See [Indices](../indices.md) for the list of built-in indices and how they are rebuilt.

## Projections

A projection is a read model derived from events. It lives in its own store, keeps its own versions and can be rebuilt from scratch by replaying events through [`IEventStorePlayer`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/IEventStorePlayer.cs). See [Projections](../projections/README.md) and [Handlers/Projections](../domain-modeling/handlers/projections.md).

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **must** pick a backend that supports append-only semantics — Cassandra is the recommended choice
* you **should** treat the event store as the only durable source of truth; everything else is derived
* you **can** add new backends behind `IEventStore` without changing your domain code
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** mutate events in place; changes to events require an explicit migration
* you **must not** delete events because "they are not used anymore"; they are still needed to rebuild projections and to audit history
* you **should not** bypass `AggregateRepository` when writing from domain code; direct `IEventStore.AppendAsync` skips integrity checks
{% endhint %}
