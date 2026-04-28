# Copy EventStore

`CopyEventStore` is the migration runner you use when the goal is _"take every event from store A, optionally transform it, write it into store B"_. It is the workhorse behind every production migration where the event-store schema or contents need to change.

## The runner

`CopyEventStore` lives at [`Cronus/src/Elders.Cronus/Migrations/CopyEventStore.cs`](../../../../src/Elders.Cronus/Migrations/CopyEventStore.cs) and extends [`MigrationRunnerBase`](../../../../src/Elders.Cronus/Migrations/MigrationRunnerBase.cs):

```csharp
public class CopyEventStore<TSourceEventStorePlayer, TTargetEventStore>
    : MigrationRunnerBase<AggregateEventRaw, TSourceEventStorePlayer, TTargetEventStore>
    where TSourceEventStorePlayer : IEventStorePlayer
    where TTargetEventStore : IEventStore
{
    public override async Task RunAsync(IEnumerable<IMigration<AggregateEventRaw>> migrations)
    {
        PlayerOperator @operator = new PlayerOperator()
        {
            OnLoadAsync = target.AppendAsync
        };

        PlayerOptions playerOptions = new PlayerOptions();
        await source.EnumerateEventStore(@operator, playerOptions);
    }
}
```

There are two open generics — the _source player_ and the _target event store_. The player drives enumeration of the old store; the target receives each event. The runner itself is a thin loop: for every `AggregateEventRaw` the player yields, it calls `target.AppendAsync` on the destination. Each migration you pass is expected to have already inspected the raw event and returned the replacement — the runner treats the result as opaque bytes and writes them.

> The current implementation applies migrations via the `IMigration<AggregateEventRaw>` pipeline your host wires into the player operator chain. Expect to see a small migrator service that sets this up — see [Migrations](README.md) for the overall topology.

## An end-to-end example

A migration we ran in production: an event contained a struct `Cycle` with a `TimeZoneInfo` member. Serialising `TimeZoneInfo` produced roughly 6000 lines of JSON per event, because every Windows time-zone record is serialised in full. The fix was to replace the `TimeZoneInfo` with a `string` timezone id. Because the old, bloated shape was already in production, changing only the code was not enough — the persisted bytes had to be rewritten.

### Step 1 — change the contract

The struct definition kept its `DataContract` name (it is the identity of the contract, not of the field) but its last member changed:

```csharp
[DataContract(Namespace = BC.ContextName, Name = "dce741fb-8671-42b8-af59-d30aaae27bad")]
public struct Cycle
{
    [DataMember(Order = 1)] private DateTimeOffset _start;
    [DataMember(Order = 2)] private DateTimeOffset _end;
    [DataMember(Order = 3)] private TimeSpan _duration;
    [DataMember(Order = 4)] private readonly string _timezoneId;
}
```

### Step 2 — write the migration

Implement [`IMigration<AggregateEventRaw>`](../../../../src/Elders.Cronus/Migrations/IMigration.cs) and mutate the serialized payload so that every occurrence of the old `Cycle` shape is rewritten with the new `_timezoneId`:

```csharp
public class TimeZoneInfoToIdMigration : IMigration<AggregateEventRaw>
{
    public bool ShouldApply(AggregateEventRaw current)
    {
        // Cheap test on the raw bytes: only apply when the old shape marker is present.
        // Keep this fast — it runs for every event in the source store.
        return BytesContain(current.Data, OldCycleMarker);
    }

    public AggregateEventRaw Apply(AggregateEventRaw current)
    {
        byte[] rewritten = RewriteCyclePayload(current.Data);
        return new AggregateEventRaw(
            current.AggregateRootId,
            rewritten,
            current.Revision,
            current.Position,
            current.Timestamp);
    }
}
```

### Step 3 — stand up a migrator host

The migrator runs in its own process with `Cronus:MigrationsEnabled = true` (see [Configuration](../../configuration.md#cronus-migrationsenabled)). Two things happen concurrently:

1. The migrator subscribes to the live event stream of the old service. Every incoming `AggregateCommit` goes through [`CronusMigrator.MigrateAsync`](../../../../src/Elders.Cronus/Migrations/CronusMigrator.cs), which applies each `IMigration<AggregateCommit>` in order and then writes the result into the new store. This keeps the new store consistent while the backfill runs.
2. A background task runs `CopyEventStore.RunAsync(new[] { new TimeZoneInfoToIdMigration() })` against the historical data of the old store. The source player enumerates every event; the runner appends each one (rewritten or not) to the target.

### Step 4 — validate, cut over, retire

Once both parts have finished — the live-subscription and the historical copy — run [`ValidateEventStore`](../../../../src/Elders.Cronus/Migrations/ValidateEventStore.cs) to confirm the old and new stores agree, flip production traffic over to the new store, and retire the old one. Keep the old store around for at least one billing cycle in case you need to reopen the investigation.

## Notes

* The source and target of `CopyEventStore` are open generics, so you can copy across backends. A typical cross-backend migration is Cassandra → new Cassandra keyspace; you can also move from one store implementation to another (`Cronus.Persistence.MSSQL` → `Cronus.Persistence.Cassandra`).
* `CopyEventStore` is idempotent at the _event_ level only if the target event store treats re-appends as a no-op. In practice Cassandra's primary-key deduplication does the right thing, but if you re-run a copy you should expect the runner to rewrite already-migrated events.
* The pagination token and retries are the responsibility of the source `IEventStorePlayer`; see [EventStore Player](../eventstore-player.md) for the hooks you will wire up.

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **should** keep `ShouldApply` cheap — it runs for every event in the store
* you **should** produce deterministic output in `Apply`; re-running the migration must produce byte-for-byte identical results
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** run `CopyEventStore` against your production host; stand up a dedicated migrator
* you **must not** rewrite the `DataContract` name of an event during a copy — keep the contract id, change the payload
{% endhint %}
