# Ports in the Cronus Framework

In the Cronus framework, **Ports** facilitate communication between aggregates, enabling one aggregate to react to events triggered by another. This design promotes a decoupled architecture, allowing aggregates to interact through well-defined events without direct dependencies.

## Key Characteristics of Ports

- **Event-Driven Communication:** Ports listen for domain events—representing business changes that have already occurred—and dispatch corresponding commands to other aggregates that need to respond.

- **Statelessness:** Ports do not maintain any persistent state. Their sole responsibility is to handle the routing of events to appropriate command handlers.

## When to Use Ports

Ports are ideal for straightforward interactions where an event from one aggregate necessitates a direct response from another. However, for more complex workflows involving multiple steps or requiring state persistence, implementing a **Saga** is recommended. Sagas provide a transparent view of the business process and manage the state across various interactions, ensuring consistency and reliability.

## Communication Guide Table

| Triggered by | Description                                           |
|--------------|-------------------------------------------------------|
| Event        | Domain events represent business changes that have already happened. |

By utilizing Ports appropriately, developers can design systems that are both modular and maintainable, adhering to the principles of Domain-Driven Design and Event Sourcing.

**Port example**

```csharp
[DataContract(Name = "a44e9a38-ab13-4f86-844a-86fefa925b53")]
public class AlertPort : IPort,
    IEventHandler<UserCreated>
{
    public Task HandleAsync(UserCreated @event)
    {
        //Implement your custom logic here
        return Task.CompletedTask;
    }
}
```