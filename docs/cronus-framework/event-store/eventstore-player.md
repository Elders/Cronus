# EventStore Player

The event-store _player_ is the read counterpart of `IEventStore` used by rebuilds. Where `IEventStore.LoadAsync(IBlobId)` loads the stream of a single aggregate, the player enumerates the whole store — scoped by event type, timestamp window and pagination token — without caring about aggregate boundaries. It is the mechanism that powers projection rebuilds, index rebuilds and public-event replay.

## The contract

The player contract lives at [`Cronus/src/Elders.Cronus/EventStore/IEventStorePlayer.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/IEventStorePlayer.cs):

```csharp
public interface IEventStorePlayer
{
    Task EnumerateEventStore(PlayerOperator @operator, PlayerOptions replayOptions, CancellationToken cancellationToken = default);
}
```

A caller hands the player two values:

* A [`PlayerOperator`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/IEventStorePlayer.cs) — a bag of callbacks the player invokes per raw event, per aggregate stream and on pagination checkpoints. The two you will set most often are `OnLoadAsync` (one raw event at a time) and `NotifyProgressAsync` (checkpoint signal, used to persist a pagination token between batches).
* A [`PlayerOptions`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/ReplayOptions.cs) — `EventTypeId`, `PaginationToken`, `BatchSize` (defaults to `1000`), `After` and `Before` bounds and `MaxDegreeOfParallelism` (defaults to `2`).

Raw events travel as [`AggregateEventRaw`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/AggregateEventRaw.cs) so a player can move bytes without deserialising them. The typed variant [`AggregateStream`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/EventStream.cs) groups raw events of one aggregate into `AggregateCommitRaw` by revision — useful when you want to see a single aggregate's commits together.

## How it is used

The player is the workhorse under several framework jobs:

* [`RebuildProjection_Job`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Rebuilding/RebuildProjection_Job.cs) iterates once per event type that the projection handles and calls `EnumerateEventStore` for each. The `OnLoadAsync` callback deserialises the raw bytes, writes a projection commit and updates the [`ProgressTracker`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Rebuilding/ProgressTracker.cs). `NotifyProgressAsync` persists the pagination token into job data so the work survives a process restart.
* [`ReplayPublicEvents_Job`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Players/ReplayPublicEvents_Job.cs) republishes the stream of one public-event type so a newly-added subscriber can catch up.
* The index rebuild jobs under [`Cronus/src/Elders.Cronus/EventStore/Index/`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/) use the same mechanism to walk every event and update the secondary indices.

The typed sibling `IEventStorePlayer<TSettings>` exists so that different backends can register their own players into DI without clashing.

## When to use it

Reach for the player when you need to walk every event of a given type (or a time slice of them) and do something idempotent with each — rebuild a projection, backfill a new index, republish a stream to a new subscriber. The player already knows how to paginate, resume from a token and report progress.

Reach for a one-off [migration](migrations/README.md) instead when you need to _transform_ the contents of the store — copy it into a new keyspace with some events rewritten, or remove events that were persisted in error. Migrations give you a destination store; the player only reads.

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **should** make `OnLoadAsync` idempotent; a replay may re-deliver events if the process restarts
* you **should** persist the pagination token via `NotifyProgressAsync` if you run long replays
* you **can** cap the blast radius with `After` / `Before` when backfilling historical data
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** mutate the source store from within a player — use a [migration](migrations/README.md) if you need that
* you **should not** assume an event is delivered exactly once; design for at-least-once
{% endhint %}
