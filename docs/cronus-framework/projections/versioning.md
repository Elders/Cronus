# Projection versioning

A projection's shape is not stable across the lifetime of a service. You add a new field to the state; you subscribe to a new event; you change the way an existing event updates the state. The projection code is different after the change — but the rows already in the projection store were written by the _old_ code, against the _old_ shape, and querying them from the new code would return wrong data. Projection versioning is Cronus's answer to that: every shape change is a new version, old versions keep serving reads until the new one is ready, and the switchover is atomic.

## What changes between versions

The projection _hash_ is the fingerprint of the projection's handler type. It is computed by [`ProjectionHasher`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/ProjectionHasher.cs) from the type's contract id plus the set of `IEventHandler<T>` interfaces it implements. When the hash changes, Cronus considers the projection's shape to have changed.

A [`ProjectionVersion`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/ProjectionVersion.cs) bundles three pieces of information: the projection's contract id (`ProjectionName`), a numeric `Revision` and the current `Hash`. It also carries a `Status` — `New`, `Building`, `Fixing`, `Live`, `Canceled`, `Timedout`, `NotPresent` or `Unknown` — which drives the lifecycle.

## The lifecycle

Four system components cooperate to turn a shape change into a safe replay:

* [`ProjectionVersionManager`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/ProjectionVersionManager.cs) — the aggregate. One instance per projection contract id per tenant, it owns the list of known versions and decides whether a new version should be requested.
* [`MarkupInterfaceProjectionVersioningPolicy`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/MarkupInterfaceProjectionVersioningPolicy.cs) — the default policy. A projection is versionable unless it implements the marker `INonVersionableProjection` (system projections opt out this way).
* [`VersionRequestTimebox`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/VersionRequestTimebox.cs) — the time window within which a version request is expected to complete. If the request has not finished by `FinishRequestUntil`, the manager cancels it.
* [`ProjectionBuilder`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/Handlers/ProjectionBuilder.cs) — the system saga that turns a `ProjectionVersionRequested` event into actual work. It decides whether the projection is "fast" (`IAmEventSourcedProjectionFast` or `IProjectionDefinition`) or sequential, builds the appropriate job, and hands it to the cluster.

The end-to-end flow:

1. **Detect change.** When the host starts, each projection's hash is computed. If it differs from the hash currently marked `Live` for that projection, `ProjectionVersionManager.NotifyHash(hash, policy, replayOptions)` is called.
2. **Request a version.** The manager checks that no replay is already in progress, then calls `Replay(hash, policy, replayOptions)`, which applies a `ProjectionVersionRequested` event with a fresh [`VersionRequestTimebox`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/VersionRequestTimebox.cs) that starts immediately and expires effectively "never" (the default is `int.MaxValue` milliseconds into the future).
3. **Start the builder.** The `ProjectionBuilder` saga handles `ProjectionVersionRequested` and schedules a `CreateNewProjectionVersion` timeout at the requested start. When it fires, the saga calls `GetJob(version, options, timebox)` to choose between [`RebuildProjection_Job`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Rebuilding/RebuildProjection_Job.cs) (fast — for event-sourced projections) and `RebuildProjectionSequentially_Job` (sequential — for projections that require strict event ordering), then runs the job through the [`ICronusJobRunner`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/ICronusJobRunner.cs).
4. **Replay events.** Inside the job, [`ProgressTracker.InitializeAsync`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Rebuilding/ProgressTracker.cs) seeds the counter from the [`IMessageCounter`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/IMessageCounter.cs) so you can display "X of Y events processed". The job walks each event type the projection handles, calls [`IEventStorePlayer.EnumerateEventStore`](../event-store/eventstore-player.md) with the type id, deserialises each raw event and writes a projection commit into the new version's storage slot. Progress is pinged to the cluster on every page.
5. **Announce milestones.** A [`RebuildProjectionStarted`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Signals/RebuildProjectionStarted.cs) signal goes out when the replay starts; [`RebuildProjectionProgress`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Signals/RebuildProjectionProgress.cs) is published once a second for monitoring; [`RebuildProjectionFinished`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Signals/RebuildProjectionFinished.cs) marks the end.
6. **Promote to live.** When the job returns `JobExecutionStatus.Completed`, the saga publishes `FinalizeProjectionVersionRequest`. The manager handles it by applying `NewProjectionVersionIsNowLive`, which moves the new version's `Status` from `Building` to `Live`. From that moment `IProjectionReader` answers queries from the new version.
7. **Retain old versions.** The previous live version is not dropped. The Cassandra projection store keeps old versions around according to the `Cronus:Projections:Cassandra:TableRetention:*` options (see [Configuration](../configuration.md#cronus-projections-cassandra)). By default `DeleteOldProjectionTables` is `false`, so nothing is deleted; when enabled, `NumberOfOldProjectionTablesToRetain` keeps that many historical versions around before garbage-collecting the oldest.

## What happens if something goes wrong

* **Timeout.** If the timebox expires before the replay completes, `ProjectionVersionManager.VersionRequestTimedout` fires a `ProjectionVersionRequestTimedout` event and the version's status becomes `Timedout`. A new request can then be issued.
* **Cancelled replay.** Operators can pause a replay through `ProjectionVersionRequestPaused`; the builder responds by calling `jobRunner.JobManager.CancelAsync(job.Name)`.
* **Outdated building version.** If a newer version of the projection is already `Live` by the time a `Building` one catches up, `CancelVersionRequest` retires the stale one.
* **Disaster recovery.** For system projections like [`ProjectionVersionsHandler`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Versioning/Handlers/ProjectionVersionsHandler.cs), `Rebuild(hash, policy, options)` bypasses the usual checks and forces a fresh rebuild, because the versioning subsystem itself depends on this projection being correct.

## A short example

You rarely call versioning APIs yourself — the framework orchestrates the whole dance. What you do is add the new field to the state, change the `HandleAsync`, deploy, and let Cronus notice:

```csharp
[DataContract(Name = "c94513d1-e5ee-4aae-8c0f-6e85b63a4e03")]
public class TaskProjection : ProjectionDefinition<TaskProjectionData, TaskId>,
    IEventHandler<TaskCreated>,
    IEventHandler<TaskCompleted> // new — previously only TaskCreated
{
    public TaskProjection()
    {
        Subscribe<TaskCreated>(x => new TaskId(x.Id.Tenant, x.Id.Id));
        Subscribe<TaskCompleted>(x => new TaskId(x.Id.Tenant, x.Id.Id));
    }

    public Task HandleAsync(TaskCreated @event) { /* ... */ return Task.CompletedTask; }
    public Task HandleAsync(TaskCompleted @event) { /* updated state shape */ return Task.CompletedTask; }
}
```

Next deploy, Cronus hashes the new handler, notices the hash has changed, requests a new `ProjectionVersion` and kicks off a replay. The old version keeps answering reads until the new one is `Live`.

## Related

* [Handlers / Projections](../domain-modeling/handlers/projections.md) — how to write the handler itself.
* [Jobs](../jobs.md) — the job runner the replay sits on top of.
* [Indices](../indices.md) — specifically the `EventToAggregateRootId` index, which the rebuild depends on to locate events efficiently.
* [Snapshots](snapshots.md) — note: snapshots are not currently shipped; that page documents the situation.
