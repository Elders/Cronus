# Commands

A command is used to dispatch domain model changes. It can be accepted or rejected depending on the domain model invariants.

## Communication Guide Table

| Triggered by | Description |
| :---: | :--- |
| UI | It is NOT common practice to send commands directly from the UI. Usually the UI communicates with web APIs |
| API | APIs sit in the middle between UI and Server translating web requests into commands |
| External System | It is NOT common practice to send commands directly from the External System. Usually the External System communicates with web APIs. |
| Port | Ports are a simple way for an aggregate to communicate with other aggregates. |
| Saga | Sagas are a simple way for an aggregate root to do complex communication with other aggregate roots. |
|  |  |

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* a command **must** be immutable
* a command **must** clearly state a business intent with a name in imperative form
* a command **can** be rejected due to domain validation, error or other reason
* a command **must** update only one AggregateRoot
{% endhint %}

## Examples

```csharp
public class DeactivateAccount : ICommand
{
    DeactivateAccount() {}
    public DeactivateAccount(AccountId id, Reason reason)
    {
        Id = id;
        Reason = reason;
    }

    public AccountId Id { get; private set; }
    public Reason ReasonToDeactivate { get; private set; }
}

[DataContract(Name = "24c59143-b95e-4fd6-8bbf-8d5efffe3185")]
public class AccountId : StringTenantId
{
    protected AccountId() { }
    public AccountId(string id, string tenant) : base(id, "account", tenant) { }
    public AccountId(IUrn urn) : base(urn, "account") { }
}

public class Reason : ValueObject<Reason>{...}
```



