# Application Services

 This is a handler where commands are received and delivered to the addressed AggregateRoot. We call these handlers _ApplicationService_. This is the _write side_ in CQRS.

## Communication Guide Table

| Triggered by | Description |
| :--- | :--- |
| Command | A command is used to dispatch domain model changes. It can either be accepted or rejected depending on the domain model invariants |

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* an appservice **can** load an aggregate root from the event store
* an appservice **can** save new aggregate root events to the event store
* an appservice **can** establish calls to the ReadModel \(not common practice but sometimes needed\)
* an appservice **can** establish calls to external services
* you **can** do dependency orchestration
* an appservice **must** be stateless
* an appservice **must** update only one aggregate root. Yes, this means that you can create one aggregate and update another one but think twice
{% endhint %}

{% hint style="warning" %}
**You should not...**

* an appservice **should not** update more than one aggregate root in single command/handler
* you **should not** place domain logic inside an application service
* you **should not** use application service to send emails, push notifications etc. Use Port or Gateway instead
* an appservice **should not** update the ReadModel
{% endhint %}

## Examples

```csharp
public class AccountAppService : AggregateRootApplicationService<Account>,
    ICommandHandler<RegisterAccount>,
    ICommandHandler<ActivateAccount>,
    ICommandHandler<SuspendAccount>,
    ICommandHandler<ResetAccountPassword>,
    ICommandHandler<ChangeAccountEmail>,
    ICommandHandler<ChangeAccountUsername>
{
    public void Handle(SuspendAccount message)
    {
        Update(message.Id, account => account.Suspend());
    }

    ...
}
```

