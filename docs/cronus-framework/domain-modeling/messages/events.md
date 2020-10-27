# Events

An event is something that has happened in the past. A domain event indicates that something happened in the domain that you want other parts of the same domain to be aware of. The domain event encapsulates all relevant data with the action that happened. It is responsible for validating its own state.

{% hint style="success" %}
**You can/should/must...**

* an event **must** be immutable
* an event **must** represent a domain event which already happened with a name in the past tense
* an event **can** be dispatched only by one aggregate
{% endhint %}

To create an event with Cronus, just use the `IEvent` markup interface.

```csharp
// TODO: give a relevant example
[DataContract(Name = "d2b92ca6-34bc-4670-890e-8dff6de624b6")]
public class ExampleCreated : IEvent
{
    public ExampleCreated(ExampleId id, ExampleName name)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (name is null) throw new ArgumentNullException(nameof(name));

        Id = id;
        Name = name;
    }

    [DataMember(Order = 1)]
    public ExampleId Id { get; private set; }

    [DataMember(Order = 2)]
    public ExampleName Name { get; private set; }

    public override string ToString()
    {
        return $"Example with id '{Id}' and name '{Name}' has been created";
    }
}
```

{% hint style="info" %}
Cronus uses the `ToString()` method for logging, so you can override it to generate user-readable logs. Otherwise, the name of the event class will be used for log messages. 
{% endhint %}

