# Indices

The event store is partitioned by aggregate id, which is exactly what you want for the write side — loading every commit of an aggregate by id is a single hot read. It is not what you want for read queries that are shaped differently ("every event of type X", "every commit whose payload references aggregate Y"). Cronus maintains a set of _secondary indices_ to answer those queries without scanning the whole store.

The index subsystem lives under [`Cronus/src/Elders.Cronus/EventStore/Index/`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/). Three indices ship with the framework.

## What is an index

An `IEventStoreIndex` is a write-time hook: every time a message is processed, Cronus dispatches it through the list of registered indices and each index records what it needs to.

```csharp
public interface IEventStoreIndex : IMessageHandler
{
    Task IndexAsync(CronusMessage message);
}

public interface ICronusEventStoreIndex : IEventStoreIndex, ISystemHandler
{ }
```

See [`IEventStoreIndex`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/IEventStoreIndex.cs). Implementations are system handlers — Cronus owns them; you do not write your own indices in application code.

The payloads each index writes live in an `IIndexStore` (and an `IIndexStatusStore` tracks the lifecycle — `NotPresent` / `Building` / `Present`, expressed by [`IndexStatus`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/IndexStatus.cs)).

## The shipping indices

### EventToAggregateRootId

File: [`EventToAggregateRootId.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/EventToAggregateRootId/EventToAggregateRootId.cs). Contract id `3d59f948-870f-4b12-ada6-9603627aaab6`.

This index records _"event of type X was written, here is the aggregate root id and the revision"_. It answers the question "give me every event of type X" — which is the primary access pattern of projection rebuilds: a projection interested in events of a given type walks this index to find the aggregates it should load events from. Public events are indexed too, but only when the event originates in the host's own bounded context (so a subscriber is not indexed for a foreign public event passing through).

### MessageCounterIndex

File: [`MessageCounterIndex.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/MessageCounterIndex.cs). Contract id `f8c532eb-57ad-469f-9002-6c286bdd88f2`.

A counter of "how many events of this type does the store contain". Used to display progress of a rebuild (`counter / total`) — the [`ProgressTracker`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Rebuilding/ProgressTracker.cs) reads the counter to compute the total it needs to chew through. The counter is updated through `IMessageCounter.IncrementAsync`.

### ProjectionIndex

File: [`ProjectionIndex.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/ProjectionIndex.cs). Contract id `37336a18-573a-4e9e-b4a2-085033b74353`.

This one bridges events and projections. When a `CronusMessage` arrives, the index looks up every registered projection type, checks whether any of its `IEventHandler<T>` interfaces would accept the message, and — if so — routes the message to `IProjectionWriter.SaveAsync(projectionType, event)`. It is the mechanism behind the live projection updates.

## Lifecycle

An index transitions through three states:

* `NotPresent` — the index has never been built (new tenant, fresh deploy, or it has been dropped).
* `Building` — an index-rebuild job is running.
* `Present` — the index is up to date.

The state lives in [`EventStoreIndexStatus`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/Handlers/EventStoreIndexStatus.cs) — a system projection whose contract id is `1bcdb806-dbd0-45e7-b781-e3d2fd0589c1`. Its `State.Status` moves from `NotPresent` to `Building` (on `EventStoreIndexRequested`) to `Present` (on `EventStoreIndexIsNowPresent`).

## Rebuilding

Rebuilding an index is orchestrated by [`EventStoreIndexBuilder`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/Handlers/EventStoreIndexBuilder.cs) — a system saga that reacts to `EventStoreIndexRequested` and schedules the appropriate rebuild job:

* [`RebuildIndex_EventToAggregateRootId_Job`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/EventToAggregateRootId/RebuildIndex_EventToAggregateRootId_Job.cs) for the `EventToAggregateRootId` index.
* [`RebuildIndex_MessageCounter_Job`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/RebuildIndex_MessageCounter_Job.cs) for the message-counter index.

The saga routes between them based on the requested index's contract id. The rebuild jobs reuse the framework-wide [Jobs](jobs.md) machinery, so they survive process restarts, coordinate across cluster nodes and report progress via `IClusterOperations.PingAsync`.

Each job enumerates the event store through an [`IEventStorePlayer`](event-store/eventstore-player.md), checkpoints its pagination token into `IJobData` and keeps the index status at `Building` until the last page has been read — at which point the saga finalises the request and publishes `EventStoreIndexIsNowPresent`, taking the index to `Present`.

### Triggering a rebuild manually

The saga reacts to a `RebuildIndexCommand` (see [`RebuildIndexCommand.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/Commands/RebuildIndexCommand.cs)). From the administration-side tooling you publish it against the `EventStoreIndexManager` aggregate for the target tenant, wait for the saga to run its course, and monitor `EventStoreIndexStatus` for the state change to `Present`.

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **should** rebuild the `EventToAggregateRootId` index first when bootstrapping a new tenant; projection rebuilds depend on it
* you **should** monitor the `IndexStatus` projection and alert if an index stays in `Building` longer than expected
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** write your own `IEventStoreIndex` — the subsystem is system-owned
* you **should not** trigger an index rebuild during peak hours; it competes with live traffic for I/O
{% endhint %}
