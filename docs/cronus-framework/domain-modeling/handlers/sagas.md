---
description: Sometimes called a Process Manager
---

# Sagas in the Cronus Framework

A saga is a message handler that watches the event stream and, as a reaction, publishes new commands. Its job is to coordinate a business process that spans more than one aggregate, or to schedule work for the future.

In Cronus, a saga is any class that implements [`ISaga`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/ISaga.cs). Nearly every real saga derives from the abstract `Saga` base class in the same file, which wires up the two publishers a saga needs:

```csharp
public abstract class Saga : ISaga
{
    protected readonly IPublisher<ICommand> commandPublisher;
    protected readonly IPublisher<IScheduledMessage> timeoutRequestPublisher;

    public Saga(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher)
    {
        this.commandPublisher = commandPublisher ?? throw new ArgumentNullException(nameof(commandPublisher));
        this.timeoutRequestPublisher = timeoutRequestPublisher ?? throw new ArgumentNullException(nameof(timeoutRequestPublisher));
    }

    public Task RequestTimeoutAsync<T>(T timeoutMessage) where T : IScheduledMessage
    {
        return timeoutRequestPublisher.PublishAsync(timeoutMessage, timeoutMessage.PublishAt);
    }
}
```

A saga reacts to events through the standard [`IEventHandler<T>`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/IEventHandler.cs) interface. On top of that, it can receive scheduled messages through [`ISagaTimeoutHandler<T>`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/ISaga.cs), where `T : IScheduledMessage`.

## Saga vs Application Service

Both are message handlers, but they sit on opposite sides of the command/event boundary.

| | Application Service | Saga |
| --- | --- | --- |
| Reacts to | `ICommand` | `IEvent`, `IScheduledMessage` |
| Produces | New events on one aggregate | New `ICommand` messages |
| Loads aggregate state? | Yes — from the event store | No |
| Purpose | Fulfil a single command | Orchestrate a multi-aggregate process |

An Application Service mutates one aggregate. A Saga never loads an aggregate directly — it publishes commands so that the appropriate Application Services do that work. If you catch yourself wanting to load aggregate state inside a saga, you almost certainly belong in an Application Service or in a new projection.

## Timeouts

A saga often needs to do something later — "if the customer has not confirmed in 24 hours, cancel the reservation". It expresses this by publishing an [`IScheduledMessage`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/ISaga.cs):

```csharp
public interface IScheduledMessage : IMessage
{
    /// <summary>
    /// The date when this message will be published.
    /// </summary>
    DateTime PublishAt { get; }
}
```

Call `RequestTimeoutAsync(message)` on the base class. Cronus will deliver the message back to the saga at `PublishAt`, and the saga handles it through `ISagaTimeoutHandler<T>`.

## A small saga

```csharp
[DataContract(Name = "d4eb8803-2cc7-48dd-9ca1-4512b8d9b88f")]
public class WelcomeSaga : Saga,
    IEventHandler<UserCreated>,
    ISagaTimeoutHandler<SendWelcomeMessageTimeout>
{
    public WelcomeSaga(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher)
        : base(commandPublisher, timeoutRequestPublisher)
    {
    }

    public Task HandleAsync(UserCreated @event)
    {
        var timeout = new SendWelcomeMessageTimeout
        {
            UserId = @event.Id,
            PublishAt = DateTime.UtcNow.AddDays(1)
        };

        return RequestTimeoutAsync(timeout);
    }

    public Task HandleAsync(SendWelcomeMessageTimeout timeout)
    {
        return commandPublisher.PublishAsync(new SendWelcomeMessage(timeout.UserId));
    }
}

[DataContract(Name = "543e8e28-0dcb-4d41-98de-f701e403dbb2")]
public class SendWelcomeMessageTimeout : IScheduledMessage
{
    [DataMember(Order = 1)] public UserId UserId { get; set; }
    [DataMember(Order = 2)] public DateTime PublishAt { get; set; }
}
```

## A real saga

A production saga rarely has a single handler. It usually reacts to a broad slice of the lifecycle it owns. For a realistic shape look at [`ExperienceVersionApprovalSaga`](https://github.com/Elders/locus.backend/blob/master/src/Elders.Locus/ExperinceApproval/Sagas/ExperienceVersionApprovalSaga.cs) in the Locus backend — it handles 13 events covering publish-for-approval, approval confirmation, rejection and every edit that invalidates a pending version — and in every case its only job is to publish a command to the right aggregate:

```csharp
[DataContract(Name = "0528cd68-62f8-40e8-b258-72ced75d0f03")]
public class ExperienceVersionApprovalSaga : Saga,
    IEventHandler<ExperienceVersionPublishedForApproval>,
    IEventHandler<ApprovalWasConfirmed>,
    IEventHandler<ExperienceVersionCanceled>,
    IEventHandler<ExperienceApprovalWasRejected>,
    IEventHandler<ExperienceVersionInfoWasAdded>,
    IEventHandler<LocationInfoWasAdded>,
    /* ... nine more events ... */
```

The saga does not touch aggregate state. Each handler inspects the event, decides on the next command and publishes it through `commandPublisher`. That is the whole pattern.

## Configuration

The subscriber that feeds sagas is toggled by `Cronus:SagasEnabled` (default: `true`). Turn it off on processes that must not advance saga state.

{% content-ref url="../../configuration.md" %}
[configuration.md](../../configuration.md)
{% endcontent-ref %}

## Best Practices

- A Saga can send new commands to drive the process forward.

* a saga **can** subscribe to events from any aggregate in the bounded context
* a saga **can** publish new commands through `commandPublisher`
* a saga **can** schedule timeouts through `RequestTimeoutAsync`
* a saga **must** be idempotent — the same event may arrive more than once
* a saga **must** decide what to do from the incoming event alone, not from external state
{% endhint %}

{% hint style="warning" %}
**You should not...**

* a saga **should not** load or mutate aggregate state — publish a command instead
* a saga **should not** send emails, call external APIs or touch the file system. Use a port or a gateway for that
* a saga **should not** write to the read model — that is a projection's job
{% endhint %}
