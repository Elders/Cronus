# Ports in the Cronus Framework

A port is the place where an event from the domain meets the outside world. It reacts to events and does I/O: sending emails, calling third-party APIs, writing to disk, pushing a message to another system, or publishing a follow-up command to a related aggregate.

In Cronus, a port is any class that implements [`IPort`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/IPort.cs):

```csharp
public interface IPort : IMessageHandler { }

public abstract class Port : IPort
{
    protected readonly IPublisher<ICommand> publisher;

    public Port(IPublisher<ICommand> publisher)
    {
        this.publisher = publisher;
    }
}
```

The `Port` base class gives you a command publisher for convenience; you are not required to use it. What makes a class a port is the `IPort` marker plus one or more [`IEventHandler<T>`](https://github.com/Elders/Cronus.DomainModeling/blob/master/src/Elders.Cronus.DomainModeling/IEventHandler.cs) implementations.

## Why a port and not an application service?

Keep Application Services focused on a single aggregate and free of I/O. That leaves the domain model fast, testable and replayable. All the messy things — latency, failure, retries — belong in a port.

A port is a natural retry boundary. If the third-party API is down, the port fails, the message is not acknowledged, and the subscriber delivers it again later. If the same side effect lived in an application service, a transient failure would also block the aggregate's write path.

## Port vs Saga

Both react to events; both can publish commands. The difference is intent:

* A **port** reaches out. Its reason to exist is the side effect — call the outside world, or fan out a single follow-up command.
* A **saga** orchestrates. Its reason to exist is a multi-step business process that spans several aggregates.

Use a port when the reaction is a one-shot "when X happens, do Y". Use a saga when Y is followed by Z and possibly a compensating W.

## Example

```csharp
[DataContract(Name = "a44e9a38-ab13-4f86-844a-86fefa925b53")]
public class WelcomeEmailPort : IPort,
    IEventHandler<UserCreated>
{
    private readonly IEmailSender emailSender;

    public WelcomeEmailPort(IEmailSender emailSender)
    {
        this.emailSender = emailSender;
    }

    public Task HandleAsync(UserCreated @event)
    {
        return emailSender.SendAsync(@event.Email, "Welcome", $"Hi {@event.Name}, welcome aboard.");
    }
}
```

And a port that reacts to an event by issuing a command to a different aggregate:

```csharp
public class RegisterUserOnAccountRegistered : IPort,
    IEventHandler<AccountRegistered>
{
    private readonly IPublisher<ICommand> commandPublisher;

    public RegisterUserOnAccountRegistered(IPublisher<ICommand> commandPublisher)
    {
        this.commandPublisher = commandPublisher;
    }

    public Task HandleAsync(AccountRegistered @event)
    {
        var userId = new UserId(@event.Id.Tenant, @event.Id.Id);
        return commandPublisher.PublishAsync(new CreateUser(userId, @event.Email));
    }
}
```

## Configuration

The subscriber that dispatches events to ports is toggled by `Cronus:PortsEnabled` (default: `true`). Turn it off on processes that must not perform outbound side effects, for example a read-only replica or a dedicated projections host.

{% content-ref url="../../configuration.md" %}
[configuration.md](../../configuration.md)
{% endcontent-ref %}

By utilizing Ports appropriately, developers can design systems that are both modular and maintainable, adhering to the principles of Domain-Driven Design and Event Sourcing.

{% hint style="success" %}
**You can/should/must...**

* a port **can** call external services (HTTP, SMTP, push services, file system, etc.)
* a port **can** publish new commands
* a port **must** be idempotent — the same event may be delivered more than once
* a port **must** tolerate the external service being slow or down — let the subscriber retry
{% endhint %}

{% hint style="warning" %}
**You should not...**

* a port **should not** load or mutate aggregate state — publish a command instead
* a port **should not** maintain persistent business state — use a gateway or a projection instead
* a port **should not** orchestrate a multi-step process — use a saga
{% endhint %}
