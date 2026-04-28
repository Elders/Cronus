# Events

A domain **event** is a fact: something that already happened inside the bounded context. It is the unit of change in an event-sourced system — the aggregate's state is computed by replaying its events, and projections are built by handling them.

{% hint style="success" %}
**You can / should / must**

* an event **must** be immutable
* an event **must** be named in the past tense (`TaskCreated`, `AccountSuspended`)
* an event **must** be emitted by exactly one aggregate via `Apply(IEvent)`
* an event **should** carry every piece of information a consumer might need — an event is a historical record and will never be enriched
* an event **must** keep its `[DataContract(Name = "<guid>")]` forever; renaming or retyping breaks replay
{% endhint %}

## Defining an event

Implement `IEvent`. Keep the parameterless constructor private so serializers can hydrate the instance, and expose every property with a private setter so the event is immutable from the outside.

{% code title="TaskCreated.cs" %}
```csharp
[DataContract(Name = "728fc4e7-628b-4962-bd68-97c98aa05694")]
public class TaskCreated : IEvent
{
    TaskCreated() { }

    public TaskCreated(TaskId id, UserId userId, string name, DateTimeOffset deadline, DateTimeOffset timestamp)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Deadline = deadline;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)] public TaskId Id { get; private set; }
    [DataMember(Order = 2)] public UserId UserId { get; private set; }
    [DataMember(Order = 3)] public string Name { get; private set; }
    [DataMember(Order = 4)] public DateTimeOffset Deadline { get; private set; }
    [DataMember(Order = 5)] public DateTimeOffset Timestamp { get; private set; }

    public override string ToString() => $"Task '{Name}' ({Id}) created for user {UserId}.";
}
```
{% endcode %}

{% hint style="info" %}
Cronus uses `ToString()` when writing structured logs. Override it to produce human-readable output; otherwise only the class name appears in log scopes.
{% endhint %}

## Emitting an event

Events are never published directly — they are always emitted through an aggregate root (or an entity inside one) by calling the protected `Apply` method. The aggregate's state handler (`public void When(TEvent e)`) is invoked synchronously and the event is added to the uncommitted stream. Persistence happens when the application service calls `repository.SaveAsync(aggregate)`.

```csharp
public class TaskAggregate : AggregateRoot<TaskState>
{
    TaskAggregate() { }

    public TaskAggregate(TaskId id, UserId userId, string name, DateTimeOffset deadline)
    {
        Apply(new TaskCreated(id, userId, name, deadline, DateTimeOffset.UtcNow));
    }
}
```

{% hint style="warning" %}
Do not inject `IPublisher<IEvent>` into application code. Domain events belong to the aggregate that produced them; Cronus publishes them to subscribers after the aggregate commit is persisted. Publishing events manually breaks event-sourcing guarantees.
{% endhint %}

## Subscribing to an event

Any handler that needs to react to an event (projection, saga, port, trigger) declares `IEventHandler<TEvent>`:

```csharp
public interface IEventHandler<in T> where T : IEvent
{
    Task HandleAsync(T @event);
}
```

See the dedicated handler pages for the specifics of each subscriber kind.

{% content-ref url="../handlers/projections.md" %}
[projections.md](../handlers/projections.md)
{% endcontent-ref %}
