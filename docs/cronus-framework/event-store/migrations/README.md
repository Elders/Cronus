# Migrations

A migration is a one-time transformation applied to the durable state of a Cronus service. You reach for one whenever the shape of persisted data has to change in a way that the runtime cannot repair on its own — renaming a contract that is still referenced in historical events, scrubbing a field that legally has to leave the store, splitting an event into two, or moving the entire store into a new keyspace.

## When do you migrate events vs projections

There is a hard asymmetry between event stores and projection stores in Cronus, and it drives the whole migrations design:

* **Projections are derived and disposable.** You can always drop the projection tables and replay the events back in to rebuild them. Projection-shape changes are handled by the [projection versioning](../../projections/versioning.md) mechanism, not by migrations. Cronus will detect that the shape hash changed, request a new version and replay events from the event store through the projection handler.
* **Events are forever.** Every change that ever happened to the business is recorded as an event, and the events are the only record. If an event's shape must change, you need to migrate the event-store data itself — and because the original store is the source of truth for the entire system you cannot mutate it blindly.

A rule of thumb: if you are changing the shape of a _write_ (an event, an aggregate-commit, a tenant-owned keyspace), you need a migration. If you are changing the shape of a _read_ (a projection, an index, a dashboard), you rebuild the derived artefact from the existing events.

## What Cronus provides

The abstractions live under [`Cronus/src/Elders.Cronus/Migrations/`](../../../../src/Elders.Cronus/Migrations/). The shapes you will interact with most are:

* [`IMigration<T>`](../../../../src/Elders.Cronus/Migrations/IMigration.cs) and `IMigration<T, V>` — the single-event predicate-plus-transformation:

  ```csharp
  public interface IMigration<in T, out V> : IMigration
      where T : class where V : class
  {
      bool ShouldApply(T current);
      V Apply(T current);
  }
  ```

  Implementations decide whether a given `AggregateEventRaw` or `AggregateCommit` should be rewritten, and return the replacement.
* [`MigrationRunnerBase<TDataFormat, TSourceEventStorePlayer, TTargetEventStore>`](../../../../src/Elders.Cronus/Migrations/MigrationRunnerBase.cs) — the base class all runners inherit from. It holds an `IEventStorePlayer` (the source) and an `IEventStore` (the destination) and declares `Task RunAsync(IEnumerable<IMigration<TDataFormat>> migrations)`.
* [`CopyEventStore<TSourceEventStorePlayer, TTargetEventStore>`](../../../../src/Elders.Cronus/Migrations/CopyEventStore.cs) — the runner used for the _"move everything from store A to store B, applying these transformations on the way"_ workflow. See [Copy EventStore](copy-eventstore.md) for the end-to-end description.
* [`DeleteEventStoreEvents<TSourceEventStorePlayer, TTargetEventStore>`](../../../../src/Elders.Cronus/Migrations/DeleteEventStoreEvents.cs) — the runner that walks the store and deletes events for which `migration.ShouldApply` returns true. Use it with caution.
* [`ValidateEventStore<TSourceEventStorePlayer, TTargetEventStore>`](../../../../src/Elders.Cronus/Migrations/ValidateEventStore.cs) — re-plays the source events into the target to verify that the migration preserves what it should.
* [`CronusMigrator`](../../../../src/Elders.Cronus/Migrations/CronusMigrator.cs) and [`MigrationHandler`](../../../../src/Elders.Cronus/Migrations/MigrationHandler.cs) — the live pipeline: given an incoming `AggregateCommit`, apply each registered `IMigration<AggregateCommit>` in order and then invoke `IMigrationCustomLogic.OnAggregateCommitAsync`. This is how the migrator service processes new events that arrive while a migration is still running.
* [`AggregateCommitMigrationWorkflow`](../../../../src/Elders.Cronus/Migrations/AggregateCommitMigrationWorkflow.cs) — a [`Workflow<AggregateCommit, IEnumerable<AggregateCommit>>`](../../../../src/Elders.Cronus/Migrations/MigrationWorkflowBase.cs) step that produces zero, one or many resulting commits from a single input commit.
* [`IMigrationEventStorePlayer`](../../../../src/Elders.Cronus/Migrations/IMigrationEventStorePlayer.cs) — a marker `IEventStorePlayer` that isolates the migration player from the rest of the system, so the live application is not accidentally driven by the historical replay.

The flag that turns the whole subsystem on is `Cronus:MigrationsEnabled` (see [Configuration](../../configuration.md#cronus-migrationsenabled)). It defaults to `false` because you almost never want a production host to be also a migrator.

## A sensible workflow

The pattern that comes up most in practice:

1. Stand up a separate _migrator_ host, configured with the new, target store as its `IEventStore` and the old store as its `IEventStorePlayer`.
2. Subscribe the migrator to the same live events the production host subscribes to, and write them straight into the new store through `CronusMigrator.MigrateAsync` so nothing is lost while the backfill runs.
3. Run [`CopyEventStore`](copy-eventstore.md) against the old store to copy history into the new store, applying the required `IMigration<AggregateEventRaw>` transformations on the way.
4. Run [`ValidateEventStore`](../../../../src/Elders.Cronus/Migrations/ValidateEventStore.cs) to confirm the new store is internally consistent.
5. Flip the production host to read from the new store, retire the old one.

Related packaging lives in the satellite [`Cronus.Migration.Middleware`](https://github.com/Elders/Cronus.Migration.Middleware), which ships the middleware needed to drop the above pipeline into a host.

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **must** migrate into a new store and keep the old one until you have validated the new one
* you **should** run migrations in a dedicated process (`Cronus:MigrationsEnabled = true`)
* you **should** make each `IMigration.Apply` deterministic so a re-run produces the same output
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** migrate events in place without a backup; the original is irreplaceable
* you **should not** use a migration where a projection rebuild or a new projection version would do
* you **must not** run migrations in your production host; it competes with live traffic for I/O and for the event stream
{% endhint %}
