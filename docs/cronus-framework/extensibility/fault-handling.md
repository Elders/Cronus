# Fault handling

Cronus retries on its own whenever a transient failure is worth one more attempt. There are two separate retry stacks in the codebase — knowing which one kicks in for your scenario is half of the useful knowledge on this page.

## The two stacks

### 1. Publisher retries — `RetryableOperation`

Every publisher that derives from [`Publisher<TMessage>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Publisher.cs) wraps its `Publish` call in a retry loop built from [`RetryableOperation`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/Userfull/RetryableOperation.cs):

```csharp
public abstract class Publisher<TMessage> : PublisherBase<TMessage> where TMessage : IMessage
{
    private RetryPolicy retryPolicy;

    public Publisher(IEnumerable<DelegatingPublishHandler> handlers) : base(handlers)
    {
        retryPolicy = new RetryPolicy(RetryableOperation.RetryPolicyFactory.CreateLinearRetryPolicy(5, TimeSpan.FromMilliseconds(300)));
    }

    public override bool Publish(TMessage message, Dictionary<string, string> messageHeaders)
    {
        bool isPublished = RetryableOperation.TryExecute(() => base.Publish(message, messageHeaders), retryPolicy);

        return isPublished;
    }
}
```

The default is **5 attempts with a fixed 300 ms delay** between them. The `RetryableOperation.RetryPolicyFactory` ships three delegate factories: `CreateLinearRetryPolicy`, `CreateExponentialRetryPolicy`, and `CreateInfiniteLinearRetryPolicy`. The `RetryPolicy` type this loop uses is `public delegate ShouldRetry RetryPolicy()` — a delegate, not a class. It returns a `bool` from `Publish`; there is no exception thrown when all retries are exhausted, the method simply returns `false`.

> **Note on the synchronous overload.** The snippet above is the in-repo `Publisher<TMessage>` base, which exposes a synchronous `Publish(...)` for the framework's own retry loop. The user-facing `IPublisher<T>` contract that ships in the [`Cronus.DomainModeling`](https://github.com/Elders/Cronus.DomainModeling) NuGet (11.0.x) is **async-only** — the methods you call from your code are `PublishAsync(...)`. The synchronous form is a framework-internal extension point, not part of the published surface.

### 2. Subscriber retries — `InMemoryRetryWorkflow`

Subscriber-side retries are implemented by [`InMemoryRetryWorkflow<TContext>`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/FaultHandling/InMemoryRetryWorkflow.cs), which wraps an inner workflow in a class-based `RetryPolicy` from [`Elders.Cronus.FaultHandling`](https://github.com/Elders/Cronus/tree/master/src/Elders.Cronus/FaultHandling):

```csharp
public class InMemoryRetryWorkflow<TContext> : Workflow<TContext> where TContext : class
{
    private RetryPolicy retryPolicy;

    readonly Workflow<TContext> workflow;

    public InMemoryRetryWorkflow(Workflow<TContext> workflow, ILogger logger)
    {
        this.workflow = workflow;
        var retryStrategy = new Incremental(5, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(500));//Total 3 etries
        retryPolicy = new RetryPolicy(new TransientErrorCatchAllStrategy(), retryStrategy, logger);
    }

    protected override async Task RunAsync(Execution<TContext> execution)
    {
        if (execution is null) throw new ArgumentNullException(nameof(execution));

        await retryPolicy.ExecuteActionAsync(() => workflow.RunAsync(execution.Context));
    }
}
```

This uses the richer [`RetryPolicy`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/FaultHandling/RetryPolicy.cs) class combined with an [`ITransientErrorDetectionStrategy`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/FaultHandling/ITransientErrorDetectionStrategy.cs) and a [`RetryStrategy`](https://github.com/Elders/Cronus/blob/master/src/Elders.Cronus/FaultHandling/RetryStrategy.cs). The defaults for `RetryStrategy` are:

* `DefaultClientRetryCount` — `10`
* `DefaultRetryInterval` — `1 s`
* `DefaultClientBackoff` — `10 s`
* `DefaultMaxBackoff` — `30 s`
* `DefaultMinBackoff` — `1 s`
* `DefaultRetryIncrement` — `1 s`

The three built-in strategies live in [`FaultHandling/Strategies`](https://github.com/Elders/Cronus/tree/master/src/Elders.Cronus/FaultHandling/Strategies):

* `FixedInterval` — constant delay between retries
* `Incremental` — linearly increasing delay
* `ExponentialBackoff` — randomised exponential delay

and the two built-in transient-error detection strategies are `TransientErrorCatchAllStrategy` (everything is transient) and `TransientErrorIgnoreStrategy` (nothing is transient — never retries).

## Replacing the defaults

Both stacks can be replaced; the mechanism is the same as for any other Cronus default — register your replacement with a discovery marked `CanOverrideDefaults = true`.

For the subscriber workflow, swap the concrete `InMemoryRetryWorkflow<>` registration for your own:

```csharp
public class CustomRetryWorkflowDiscovery : DiscoveryBase<Workflow<HandleContext>>
{
    protected override DiscoveryResult<Workflow<HandleContext>> DiscoverFromAssemblies(DiscoveryContext context)
    {
        var model = new DiscoveredModel(
            typeof(InMemoryRetryWorkflow<HandleContext>),
            typeof(CustomRetryWorkflow<HandleContext>),
            ServiceLifetime.Transient)
        {
            CanOverrideDefaults = true
        };

        return new DiscoveryResult<Workflow<HandleContext>>(new[] { model });
    }
}
```

For the publisher retry loop, either subclass `Publisher<TMessage>` with your own retry configuration or, in a transport, replace the publisher type altogether with a discovery — the RabbitMQ and CosmosDb satellites both do this.

## Example — a custom retry strategy

A retry strategy that only treats network-level exceptions as transient and backs off exponentially:

```csharp
public sealed class NetworkOnlyTransientStrategy : ITransientErrorDetectionStrategy
{
    public bool IsTransient(Exception ex)
        => ex is HttpRequestException or SocketException or TimeoutException;
}

public static class CustomRetryPolicies
{
    public static RetryPolicy NetworkExponential(ILogger logger)
    {
        var strategy = new ExponentialBackoff(
            retryCount: 5,
            minBackoff: TimeSpan.FromMilliseconds(250),
            maxBackoff: TimeSpan.FromSeconds(10),
            deltaBackoff: TimeSpan.FromMilliseconds(500));

        return new RetryPolicy(new NetworkOnlyTransientStrategy(), strategy, logger);
    }
}
```

Pass the resulting `RetryPolicy` into your replacement workflow. The built-in `InMemoryRetryWorkflow<T>` hard-codes `TransientErrorCatchAllStrategy` plus `Incremental` — if you want anything else, you replace the whole workflow.

## Circuit breaking with Cronus.Hystrix

For more advanced fault tolerance — circuit breakers, bulkheads, fallbacks — there is a legacy satellite called [`Cronus.Hystrix`](https://github.com/Elders/Cronus.Hystrix). It predates the current extensibility model and is documented separately; mention it here only because some older services still depend on it.

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* you **can** replace the retry workflow via a discovery marked `CanOverrideDefaults = true`
* you **should** pair a custom `RetryStrategy` with an `ITransientErrorDetectionStrategy` that tells transient from permanent errors; retrying a validation error wastes time
* you **should** log at each retry; the built-in `RetryPolicy` does this through the `ILogger` you inject
{% endhint %}

{% hint style="warning" %}
**You should not...**

* you **should not** retry forever in a subscriber — unbounded retries block the whole queue. `CreateInfiniteLinearRetryPolicy` exists but is appropriate only for the publisher loop, not for handlers
* you **should not** catch and swallow exceptions inside a handler just to avoid the retry — the subscriber cannot tell success from failure if you do
* you **should not** mix `Elders.Cronus.FaultHandling.RetryPolicy` (the class) with `Elders.Cronus.RetryPolicy` (the delegate) — they live in different namespaces on purpose
{% endhint %}
