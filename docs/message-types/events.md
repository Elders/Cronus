# Events

Domain events represent business changes which already happened.

## Communication Guide Table

| Triggered by | Description |
| :--- | :--- |
| AggregateRoot | TODO |

{% hint style="success" %}
**You can/should/must...**

* an event **must** be immutable
* an event **must** represent a domain event which already happened with a name in past tense
* an event **can** be dispatched only by one aggregate
{% endhint %}

## Examples

```csharp
[DataContract(Name = "fff400a3-1af0-4332-9cf5-b86c1c962a01")]
public class AccountSuspended : IEvent
{
    AccountSuspended() { }

    public AccountSuspended(AccountId id)
    {
        Id = id;
    }

    [DataMember(Order = 1)]
    public AccountId Id { get; private set; }

    public override string ToString()
    {
        return "Account was suspended";
    }
}
```

