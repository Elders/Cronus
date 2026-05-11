# Cluster

A Cronus service is normally deployed as multiple identical hosts behind a load balancer: the same binary runs on several machines for throughput and fault tolerance. For the message-handling path this works trivially — RabbitMQ delivers each message to exactly one of the workers and there is nothing else to coordinate. For the _long-running, singleton_ work (projection rebuilds, index rebuilds, migration sweeps, replays) the cluster has to agree on one host doing the work at a time, and it has to agree on where that work is _up to_ if the host dies in the middle of it. The cluster subsystem exists to provide that coordination.

## What clustering does

Concretely, the cluster gives you:

* **Leader election / singleton execution.** A job declares a `Name` and the cluster guarantees that only one host is actively advancing the job with that name. If that host dies, another host picks up from where the cluster last saw progress.
* **Shared job state.** A job's [`IJobData`](../../../src/Elders.Cronus/Cluster/Job/IJobData.cs) is synced to the cluster through [`IClusterOperations.PingAsync`](../../../src/Elders.Cronus/Cluster/Job/IClusterOperations.cs). When a new host starts running a job, it pings the cluster first and uses whatever state comes back.
* **Cancellation.** Each running job is registered under its name in a `JobManager` (see [`ICronusJobRunner`](../../../src/Elders.Cronus/Cluster/Job/ICronusJobRunner.cs)). Jobs can be cancelled by name from any host.
* **Health signals.** Every host emits a `HeartbeatSignal` on a configurable interval so the cluster can see which hosts are live. The interval is `Cronus:Heartbeat:IntervalInSeconds` (default `5`, validated `5..3600`) — see [Configuration](../configuration.md#cronus.heartbeat.intervalinseconds).

## The contracts

The cluster exposes two related contracts under [`Cronus/src/Elders.Cronus/Cluster/Job/`](../../../src/Elders.Cronus/Cluster/Job/):

```csharp
public interface IClusterOperations
{
    Task<TData> PingAsync<TData>(CancellationToken cancellationToken = default) where TData : class, new();
    Task<TData> PingAsync<TData>(TData data, CancellationToken cancellationToken = default) where TData : class, new();
}

public interface ICronusJobRunner : IDisposable
{
    Task<JobExecutionStatus> ExecuteAsync(ICronusJob<object> job, CancellationToken cancellationToken = default);
    JobManager JobManager { get; }
}
```

`IClusterOperations` is the _"talk to the cluster"_ side: jobs call `PingAsync(Data)` to publish progress and `PingAsync<TData>()` to fetch the last-known state. `ICronusJobRunner` is the _"execute work on this host"_ side: it registers the cancellation source under the job name and invokes the job's `RunAsync`.

Most hosts extend [`AbstractCronusJobRunner`](../../../src/Elders.Cronus/Cluster/Job/ICronusJobRunner.cs) rather than implementing the runner from scratch; the abstract base class does the cancellation book-keeping for you.

## In-memory default

Out of the box Cronus wires the cluster contracts to their no-op in-memory implementations:

* [`InMemoryCronusJobRunner`](../../../src/Elders.Cronus/Cluster/Job/InMemory/InMemoryCronusJobRunner.cs) — executes the job on the current host with no external coordination.
* [`NoClusterOperations`](../../../src/Elders.Cronus/Cluster/Job/InMemory/NoClusterOperations.cs) — `PingAsync<T>()` returns `default`; `PingAsync<T>(data)` returns the data it was given.

This is what you want during local development and single-host deployments. It is also what [`JobDiscovery`](../../../src/Elders.Cronus/Cluster/Job/JobDiscovery.cs) wires by default.

## Consul-backed cluster

The production runner is the satellite package [`Cronus.Cluster.Consul`](https://github.com/Elders/Cronus.Cluster.Consul). It replaces the in-memory runner and operations with a Consul-backed pair: job state lives in the Consul key-value store, leader election uses Consul session locks, and the heartbeat writes into Consul's health checks.

The Consul package is configured through the standard Cronus configuration surface — see [Configuration](../configuration.md) for the `cronus:cluster:*` options (connection string, acl token, namespace). When the Consul runner is registered it replaces the in-memory one in DI so existing jobs run unchanged.

## Sub-topics

{% content-ref url="jobs.md" %}
[jobs.md](jobs.md)
{% endcontent-ref %}

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **must** run a real cluster runner (Consul-backed) in production; the in-memory one is only safe for a single host
* you **should** keep job names stable across releases so the cluster can correlate progress across versions
* you **should** ping the cluster after every meaningful increment so another host can resume work
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **must not** run duplicate instances of the same singleton job without the cluster; each one will think it is alone
* you **should not** put large payloads into `IJobData`; the cluster key-value store is not a document database
{% endhint %}
