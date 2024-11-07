# Events

An event is something significant that has happened in the domain.  It encapsulates all relevant data of the action that happened.

{% hint style="success" %}
**You can/should/must...**

* an event **must** be immutable
* an event **must** represent a domain event that already happened with a name in the past tense
* an event **can** be dispatched only by one aggregate
{% endhint %}

To create an event with Cronus, just use the `IEvent` markup interface.

```csharp
[DataContract(Name = "728fc4e7-628b-4962-bd68-97c98aa05694")]
public class TaskCreated : IEvent
{
    TaskCreated() { }

    public TaskCreated(TaskId id, UserId userId, string name, DateTimeOffset timestamp)
    {
        Id = id;
        UserId = userId;
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public TaskId Id { get; private set; }

    [DataMember(Order = 2)]
    public UserId UserId { get; private set; }

    [DataMember(Order = 3)]
    public string Name { get; private set; }

    [DataMember(Order = 4)]
    public DateTimeOffset CreatedAt { get; private set; }

    [DataMember(Order = 5)]
    public DateTimeOffset Timestamp { get; private set; }

    public override string ToString()
    {
        return $"Task with id '{Id}' and name '{Name}' for user [{UserId}] at {CreatedAt} has been created.";
    }
}
```

{% hint style="info" %}
Cronus uses the `ToString()` method for logging, so you can override it to generate user-readable logs. Otherwise, the name of the event class will be used for log messages.&#x20;
{% endhint %}
