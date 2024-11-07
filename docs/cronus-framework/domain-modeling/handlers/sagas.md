---
description: Sometimes called a Process Manager
---

# Sagas in the Cronus Framework

In the Cronus framework, **Sagas**—also known as **Process Managers**—are designed to handle complex workflows that span multiple aggregates. They provide a centralized mechanism to coordinate and manage long-running business processes, ensuring consistency and reliability across the system.

## Key Characteristics of Sagas

- **Event-Driven Coordination:** Sagas listen for domain events, which represent business changes that have already occurred, and react accordingly to drive the process forward.

- **State Management:** Unlike simple event handlers, Sagas maintain state to track the progress of the workflow, enabling them to handle complex scenarios and ensure that all steps are completed successfully.

- **Command Dispatching:** Sagas can send new commands to aggregates or other components, orchestrating the necessary actions to achieve the desired business outcome.

## When to Use Sagas

Sagas are particularly useful when dealing with processes that:

- Involve multiple aggregates or bounded contexts.

- Require coordination of several steps or actions.

- Need to handle compensating actions in case of failures to maintain consistency.

By encapsulating the workflow logic within a Saga, developers can manage complex business processes more effectively, ensuring that all parts of the system work together harmoniously.

## Communication Guide Table

| Triggered by | Description                                           |
|--------------|-------------------------------------------------------|
| Event        | Domain events represent business changes that have already happened. |

## Best Practices

- A Saga can send new commands to drive the process forward.

- Ensure that Sagas are idempotent to handle potential duplicate events gracefully.

- Maintain clear boundaries for each Saga to prevent unintended side effects.

**Saga example**

```csharp
[DataContract(Name = "d4eb8803-2cc7-48dd-9ca1-4512b8d9b88f")]
public class TaskSaga : Saga,
    IEventHandler<UserCreated>,
    ISagaTimeoutHandler<Message>

{
    public TaskSaga(IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher) : base(commandPublisher, timeoutRequestPublisher)
    {
    }

    public Task HandleAsync(UserCreated @event)
    {
        var message = new Message();
        message.Info = @event.Name + "was created yesterday.";
        message.PublishAt = DateTimeOffset.UtcNow.AddDays(1).DateTime;
        message.Timestamp = DateTimeOffset.UtcNow;

        RequestTimeout<Message>(message);

        return Task.CompletedTask;
    }
    public Task HandleAsync(Message sagaTimeout)
    {
        Console.WriteLine(sagaTimeout.Info);

        return Task.CompletedTask;
    }

}

[DataContract(Name = "543e8e28-0dcb-4d41-98de-f701e403dbb2")]
public class Message : IScheduledMessage
{
    public string Info { get; set; }
    public DateTime PublishAt { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
```

