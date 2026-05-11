# Triggers

A trigger is a message handler that starts something new in response to an event or a signal. The "something new" is typically a job, a long-running workflow, or a downstream orchestration that the current message flow is not supposed to wait for.

In Cronus, a trigger is any class that implements [`ITrigger`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/ITrigger.cs):

```csharp
public interface ITrigger : IMessageHandler { }
```

The marker interface is deliberately minimal. A trigger gets its behaviour from the handler interfaces it implements on top — usually [`IEventHandler<T>`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/IEventHandler.cs) or [`ISignalHandle<T>`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/ISignalHandle.cs).

## When to use a trigger

Use a trigger when the reaction is:

* Starting a background [job](../../jobs.md) that runs on its own schedule.
* Kicking off a workflow that is not bounded by the lifetime of the incoming message.
* Fanning a signal out into the job subsystem — signals are the idiomatic message type here because they are fire-and-forget and are not meant to participate in event sourcing.

If you just need to publish a command or call the outside world, you want a [port](ports.md) or a [gateway](gateways.md) instead.

## Relationship to jobs

Jobs (see [Jobs](../../jobs.md)) run under the Cronus job manager and execute independently of the subscriber that received the original message. A trigger is the common way to connect the two worlds: the trigger receives an event or signal, decides whether a job should run, and starts it.

The trigger itself does no business-state work — it is a thin adapter between the message bus and the job subsystem.

## Example

A trigger that starts a push-notification job when a signal arrives:

```csharp
public class PushNotificationTrigger : ITrigger,
    ISignalHandle<NotificationMessageSignal>
{
    private readonly IProjectionReader projections;
    private readonly MultiPlatformDelivery delivery;
    private readonly ILogger<PushNotificationTrigger> logger;

    public PushNotificationTrigger(IProjectionReader projections, MultiPlatformDelivery delivery, ILogger<PushNotificationTrigger> logger)
    {
        this.projections = projections;
        this.delivery = delivery;
        this.logger = logger;
    }

    public Task HandleAsync(NotificationMessageSignal signal)
    {
        // resolve recipients, look up tokens, hand over to the delivery subsystem
        // ...
        return Task.CompletedTask;
    }
}
```

## Configuration

The subscriber that dispatches messages to triggers is toggled by `Cronus:TriggersEnabled` (default: `true`). Turn it off on hosts where triggers should not fire.

{% content-ref url="../../configuration.md" %}
[configuration.md](../../configuration.md)
{% endcontent-ref %}

{% content-ref url="../../jobs.md" %}
[jobs.md](../../jobs.md)
{% endcontent-ref %}

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* a trigger **can** start jobs, workflows or other long-running work
* a trigger **can** handle both events and signals
* a trigger **must** be idempotent — the same message may arrive more than once
* a trigger **should** delegate the actual work to a service injected into its constructor
{% endhint %}

{% hint style="warning" %}
**You should not...**

* a trigger **should not** publish commands as a matter of course — use a port or a saga
* a trigger **should not** mutate aggregate state
* a trigger **should not** block the subscriber on long-running work; hand it over to a job
{% endhint %}
