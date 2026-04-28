# Jobs

A Cronus _job_ is a long-running, idempotent piece of work coordinated across the cluster. Think "rebuild this projection", "rebuild this index", "replay these public events to a new subscriber" — work that cannot finish inside the timeout of a single message handler and must survive a process restart. Jobs are how those tasks are modelled.

## The subsystem

Job code lives under [`Cronus/src/Elders.Cronus/Cluster/Job/`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/). The core types are:

```csharp
public interface ICronusJob<out TData> : ICronusJobb
    where TData : class
{
    public TData Data { get; }

    Task SyncInitialStateAsync(IClusterOperations cluster, CancellationToken cancellationToken = default);
    Task<JobExecutionStatus> RunAsync(IClusterOperations cluster, CancellationToken cancellationToken = default);
}

public enum JobExecutionStatus
{
    Completed,
    Canceled,
    Failed,
    Running
}
```

See [`ICronusJob.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/ICronusJob.cs) and [`JobExecutionStatus.cs`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/JobExecutionStatus.cs). A job pairs a strongly-typed `TData` (its durable state, implementing [`IJobData`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/IJobData.cs)) with a `RunAsync` method that pushes the work forward one increment at a time.

The entry point is [`ICronusJobRunner`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/ICronusJobRunner.cs):

```csharp
public interface ICronusJobRunner : IDisposable
{
    Task<JobExecutionStatus> ExecuteAsync(ICronusJob<object> job, CancellationToken cancellationToken = default);
    JobManager JobManager { get; }
}
```

Callers build a job (typically via a factory, not by hand), hand it to the runner, and receive a `JobExecutionStatus`. The runner registers the job under its `Name` in the `JobManager` so it can be cancelled by id.

The default runner is the single-process, no-cluster [`InMemoryCronusJobRunner`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/InMemory/InMemoryCronusJobRunner.cs), which runs against a [`NoClusterOperations`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/InMemory/NoClusterOperations.cs). For the multi-host story the Consul-backed runner in [`Cronus.Cluster.Consul`](https://github.com/Elders/Cronus.Cluster.Consul) coordinates which node runs which job — see [Cluster](cluster/README.md).

## The base class

Most jobs do not implement `ICronusJob` directly; they extend [`CronusJob<TData>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/CronusJob.cs):

```csharp
public abstract class CronusJob<TData> : ICronusJob<TData>
    where TData : class, IJobData, new()
{
    public abstract string Name { get; set; }
    public TData Data { get; protected set; }

    protected abstract Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default);
}
```

The base class handles the two things every job cares about:

* `SyncInitialStateAsync` — ping the cluster for the last-known `TData` so a new host picks up the work where a previous one stopped.
* `Override(fromCluster, fromLocal)` — merge the cluster state with the local initial data. The default implementation prefers `fromCluster` unless the cluster state is older than the local one _and_ complete.

From inside `RunJobAsync` the job reads and writes `Data`, and periodically calls `cluster.PingAsync(Data)` to publish its progress. See [`IClusterOperations`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/IClusterOperations.cs) for the contract.

## Discovery

Jobs are wired up through [`JobDiscovery`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Cluster/Job/JobDiscovery.cs). It walks the assemblies, registers every implementation of `ICronusJob<object>` as a transient service, wires `ICronusJobRunner` to `InMemoryCronusJobRunner` by default, and builds a `TypeContainer<ICronusJob<object>>` so the system knows how to resolve them.

The same discovery also registers factory types for the framework-owned jobs:

* `RebuildIndex_EventToAggregateRootId_JobFactory`, `RebuildIndex_MessageCounter_JobFactory` (see [Indices](indices.md))
* `ReplayPublicEvents_JobFactory` (see [EventStore Player](event-store/eventstore-player.md))
* `RebuildProjection_JobFactory`, `RebuildProjectionSequentially_JobFactory` (see [Projections / Versioning](projections/versioning.md))

## When to write a job

Reach for a job when the work is all of:

* Idempotent — re-running the same increment against the same `TData` must be safe.
* Too long for a message handler timeout, or requires pagination through large data sets.
* Cluster-coordinated — only one host should advance it at a time.
* Stateful — progress should survive process restarts.

If the work is short, stateless and tenant-scoped, write a signal handler or a saga instead.

## A small example

A sketch of a minimal job — the only interesting thing it does is increment a counter until it reaches some target and ping the cluster each iteration:

```csharp
public sealed class CountToTenJob : CronusJob<CountToTenJobData>
{
    public CountToTenJob(ILogger<CountToTenJob> logger) : base(logger) { }

    public override string Name { get; set; } = "count-to-ten";

    protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
    {
        while (Data.Counter < 10)
        {
            Data.Counter++;
            Data.Timestamp = DateTimeOffset.UtcNow;
            Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
                return JobExecutionStatus.Canceled;
        }

        Data.IsCompleted = true;
        Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);
        return JobExecutionStatus.Completed;
    }
}

public sealed class CountToTenJobData : IJobData
{
    public bool IsCompleted { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public int Counter { get; set; }
}
```

You execute it through the runner — never by calling `RunAsync` directly, because the runner is what registers the cancellation and tracks the job name:

```csharp
var status = await jobRunner.ExecuteAsync(countToTenJob);
```

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **must** make `RunJobAsync` idempotent; the cluster will re-invoke it after failures
* you **should** ping the cluster after every meaningful state change so another host can resume the work
* you **should** check `cancellationToken` between units of work; the runner cancels jobs by name
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** use a job where a signal handler or saga would do
* you **should not** store large blobs inside `IJobData`; the cluster is not a document store
* you **should not** share mutable state between `RunJobAsync` invocations; rely on `Data` and `PingAsync`
{% endhint %}
