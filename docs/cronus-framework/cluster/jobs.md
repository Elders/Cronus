# Jobs in the cluster

The generic [Jobs](../jobs.md) page describes what a `CronusJob<TData>` is and how you write one. This page is about the part that matters only once more than one host is running: how the cluster picks who advances a job, how progress is shared, and what that means for the jobs Cronus ships with.

## Singleton semantics

The foundational guarantee the cluster makes is _"at most one host is actively advancing the job with this `Name`"_. This is the property that lets a rebuild run across many machines without double-writing projection rows, and that lets a migration copy events without duplicating them at the destination.

Implementations achieve it with different primitives. The in-memory [`InMemoryCronusJobRunner`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/InMemory/InMemoryCronusJobRunner.cs) trivially satisfies the rule because there is only one host. The Consul-backed [`Cronus.Cluster.Consul`](https://github.com/Elders/Cronus.Cluster.Consul) runner relies on Consul session locks: acquiring the session for a given job name is what gives a host the right to advance it. If the session expires — because the host died or stopped pinging — Consul releases the lock and another host can take over.

The fact that jobs are keyed by `Name` has consequences. Cronus tries hard to produce deterministic, tenant-scoped names; see, for example, how the message-counter rebuild factory builds its name:

```csharp
job.Name = $"urn:{boundedContext.Name}:{contextAccessor.CronusContext.Tenant}:{job.Name}";
```

— from [`RebuildIndex_MessageCounter_JobFactory`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/RebuildIndex_MessageCounter_Job.cs). Every job factory you meet constructs the name from the bounded context, the tenant and a stable suffix, so the cluster can correlate runs across restarts and across hosts.

## State sharing

A job's progress is an instance of `IJobData` (see [`IJobData.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/IJobData.cs) for the minimal contract — `IsCompleted` and `Timestamp`). Real jobs carry more: pagination tokens, counters, the projection version being rebuilt, the event-type id being replayed.

[`CronusJob<TData>.SyncInitialStateAsync`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/CronusJob.cs) is the sync point. When a host is about to run a job, it first calls:

```csharp
Data = await cluster.PingAsync<TData>(cancellationToken).ConfigureAwait(false);
if (Data is null) Data = BuildInitialData();
Data = DataOverride(Data);
```

If the cluster has state for this job, the host uses that state. If it does not, the host falls back to the initial data built by the factory — which typically seeds things like the timestamp when the work was first requested.

After that, every meaningful step of the work is followed by a `Data = await cluster.PingAsync(Data)`. That call does two things: it publishes the current `Data` as the cluster's authoritative state, and it returns whatever the cluster has (which may be the same or may have been overwritten concurrently, though in practice only the singleton lock-holder writes).

## Leader election

Leader election is not exposed as a first-class API — it is implicit in `ExecuteAsync`. If the runner decides this host is not currently the leader for a given job name, `ExecuteAsync` will either refuse to advance the work or it will wait for the leader to release the lock. The concrete behaviour is backend-specific (Consul returns fast if the session is taken; the in-memory runner always proceeds).

If you need to _cancel_ the work from another host, the cluster's mechanism for that is the `JobManager` — call `JobManager.CancelAsync(jobName)` on any host, and the cancellation token propagates through to the runner that currently holds the lock.

## Work distribution in shipped jobs

The jobs Cronus ships use the cluster in subtly different ways:

* [`RebuildIndex_MessageCounter_Job`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Index/RebuildIndex_MessageCounter_Job.cs) — a singleton sweep over every registered event type. It pings the cluster every 5 seconds (in a background loop) so the cluster sees the host is alive while the sweep runs.
* [`RebuildProjection_Job`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Projections/Rebuilding/RebuildProjection_Job.cs) — a singleton per projection version. Progress tokens per event type live in `Data.EventTypePaging` and are pinged to the cluster through `NotifyProgressAsync` inside the `PlayerOperator`.
* [`ReplayPublicEvents_Job`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Players/ReplayPublicEvents_Job.cs) — a singleton per `{recipient bounded context, recipient handlers, source event type}` triple. Different recipients can replay in parallel because their job names differ.

All three share the same pattern: a short `SyncInitialStateAsync`, a loop that does one unit of work at a time, a `PingAsync(Data)` after each unit, and a `IsCompleted = true; PingAsync(Data)` at the end.

## Related pages

{% content-ref url="README.md" %}
[README.md](README.md)
{% endcontent-ref %}

{% content-ref url="../jobs.md" %}
[jobs.md](../jobs.md)
{% endcontent-ref %}

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **should** keep job names deterministic and versionless; append a revision only when the semantics of the job itself change
* you **should** ping the cluster before any expensive operation so the singleton lock is kept alive
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** rely on a job running on "the same host every time"; the cluster may move it without notice
* you **must not** use `JobManager.CancelAsync` as a regular flow-control mechanism; reserve it for operator intervention
{% endhint %}
