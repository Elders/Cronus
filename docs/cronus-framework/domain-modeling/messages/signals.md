# Signals

A **signal** is a fire-and-forget trigger that tells something in the system to run. Unlike a command it has no aggregate target, no invariants, and no success/failure contract — its only purpose is to fan out work.

Cronus itself uses signals for heartbeats, projection rebuilds, and index maintenance. In your domain you might use them to pulse periodic reports, announce cron-like events, or ping a saga to re-evaluate its state.

```csharp
public interface ISignal : IMessage { }
```

{% hint style="success" %}
**You can / should / must**

* a signal **must** be immutable and self-contained
* a signal **should not** encode business decisions — it's a wake-up, not an order
* a signal **can** be broadcast to many handlers at once; design them to be independent
* a signal handler **must** be idempotent — the same signal can arrive more than once
{% endhint %}

## Defining a signal

{% code title="RefreshDailyReportSignal.cs" %}
```csharp
[DataContract(Name = "c04b3c09-4ad4-4a62-b2f6-d5d86d4f0e55")]
public class RefreshDailyReportSignal : ISignal
{
    RefreshDailyReportSignal() { }

    public RefreshDailyReportSignal(string tenant, DateTimeOffset timestamp)
    {
        Tenant = tenant;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)] public string Tenant { get; private set; }
    [DataMember(Order = 2)] public DateTimeOffset Timestamp { get; private set; }

    public override string ToString() => $"Refresh daily report for {Tenant}.";
}
```
{% endcode %}

## Handling a signal

Implement `ISignalHandle<TSignal>`:

```csharp
public interface ISignalHandle<in T> where T : ISignal
{
    Task HandleAsync(T signal);
}
```

```csharp
public class DailyReportTrigger : ITrigger,
    ISignalHandle<RefreshDailyReportSignal>
{
    private readonly IPublisher<ICommand> commandPublisher;

    public DailyReportTrigger(IPublisher<ICommand> commandPublisher)
    {
        this.commandPublisher = commandPublisher;
    }

    public async Task HandleAsync(RefreshDailyReportSignal signal)
    {
        var cmd = new RebuildDailyReport(signal.Tenant, signal.Timestamp);
        await commandPublisher.PublishAsync(cmd).ConfigureAwait(false);
    }
}
```

## Publishing a signal

Inject `IPublisher<ISignal>` and call `PublishAsync`:

```csharp
await signalPublisher.PublishAsync(new RefreshDailyReportSignal("acme", DateTimeOffset.UtcNow));
```

You can also schedule a signal with one of the `PublishAsync` overloads that accepts a `DateTime` or a `TimeSpan`.

{% hint style="info" %}
Because a signal fans out, multiple handlers can answer it. If the work must happen only once per tick, make each handler idempotent and use the signal metadata (`Timestamp`, tenant) to deduplicate.
{% endhint %}

## Examples in Cronus itself

Two real signal types ship inside the framework and are good references when you write your own.

### `HeartbeatSignal`

`CronusHeartbeat` wakes up every `Cronus:Heartbeat:IntervalInSeconds` seconds, builds a [`HeartbeatSignal`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Hosting/Heartbeat/HeartbeatSignal.cs) and publishes it through `IPublisher<ISignal>`. The signal carries the bounded context and the full tenant list, so downstream services can use it as a liveness probe — see [Observability](../../extensibility/observability.md) for the consumer side.

```csharp
[DataContract(Namespace = "cronus", Name = "c80739a6-b5dc-483e-8c11-06a85542416e")]
public sealed class HeartbeatSignal : ISignal
{
    HeartbeatSignal() { Tenants = new List<string>(); }

    public HeartbeatSignal(string boundedContext, List<string> tenants)
    {
        BoundedContext = boundedContext;
        Tenants = tenants;
        Timestamp = DateTimeOffset.Now;
        Tenant = "cronus";
        MachineName = Environment.MachineName;
        EnvironmentConfig = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    }

    [DataMember(Order = 0)] public string Tenant { get; private set; }
    [DataMember(Order = 1)] public string BoundedContext { get; private set; }
    [DataMember(Order = 2)] public List<string> Tenants { get; private set; }
    [DataMember(Order = 3)] public DateTimeOffset Timestamp { get; private set; }
    [DataMember(Order = 4)] public string MachineName { get; private set; }
    [DataMember(Order = 5)] public string EnvironmentConfig { get; private set; }
}
```

The publisher on the heartbeat side typically attaches a TTL header so a delayed consumer can drop a stale beat:

```csharp
var headers = new Dictionary<string, string> { { MessageHeader.TTL, "5000" } };
await publisher.PublishAsync(signal, headers);
```

### `PublicEventsPlayer`

[`PublicEventsPlayer`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/EventStore/Players/PublicEventsPlayer.cs) is a system trigger that handles `ReplayPublicEventsRequested` — a signal asking the host to republish a slice of the public-event stream to a newly-added subscriber. It is a small but representative example of an `ISystemTrigger` that reacts to a signal by kicking off a [job](../../jobs.md):

```csharp
public sealed class PublicEventsPlayer : ISystemTrigger,
    ISignalHandle<ReplayPublicEventsRequested>
{
    private readonly ICronusJobRunner jobRunner;
    private readonly ReplayPublicEvents_JobFactory jobFactory;
    private readonly ILogger<PublicEventsPlayer> logger;

    public async Task HandleAsync(ReplayPublicEventsRequested signal)
    {
        ReplayPublicEvents_Job job = jobFactory.CreateJob(signal);
        await jobRunner.ExecuteAsync(job).ConfigureAwait(false);
    }
}
```

The pattern — receive a signal, build a job, hand it to the runner — is the canonical way to express "kick off long-running work in response to an ambient nudge".
