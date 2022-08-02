# Application Services

This is a handler where commands are received and delivered to the addressed aggregate. Such a handler is called an application service. This is the "_write"_ side in [CQRS](../../concepts/cqrs.md).

An application service is a command handler for a specific aggregate. One aggregate has one application service whose purpose is to orchestrate how commands will be fulfilled. Its the application service's responsibility to invoke the appropriate aggregate methods and pass the command's payload. It mediates between Domain and infrastructure and it shields any domain model from the "_outside_". Only the application service __ interacts with the domain model.

{% content-ref url="../aggregate.md" %}
[aggregate.md](../aggregate.md)
{% endcontent-ref %}

You can create an application service with Cronus by using the `AggregateRootApplicationService` base class. Specifying which commands the application service can handle is done using the `ICommandHandler<T>` interface.

`AggregateRootApplicationService` provides a property of type `IAggregateRepository` that you can use to load and save the aggregate state. There is also a helper method `Update(IAggregateRootId id, Action update)` that loads and aggregate based on the provided id invokes the action and saves the new state if there are any changes.

```csharp
public class ConcertAppService : AggregateRootApplicationService<Concert>,
    ICommandHandler<AnnounceConcert>,
    ICommandHandler<RegisterPerformer>
{
    ...
    
    public void Handle(AnnounceConcert command)
    {
        if (Repository.TryLoad<Concert>(command.Id, out _))
            return;

        var concert = new Concert(...);
        Repository.Save(concert);
    }
    
    public void Handle(RegisterPerformer command)
    {
        Update(command.Id, x => x.RegisterPerformer(...));
    }

    ...
}
```

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* an application service **can** load an aggregate root from the event store
* an application service **can** save new aggregate root events to the event store
* an application service **can** establish calls to the read model (not a common practice but sometimes needed)
* an application service **can** establish calls to external services
* you **can** do dependency orchestration
* an application service **must** be stateless
* an application service **must** update only one aggregate root. Yes, you can create one aggregate and update another one but think twice before doing so.
{% endhint %}

{% hint style="warning" %}
**You should not...**

* an application service **should not** update more than one aggregate root in a single command/handler
* you **should not** place domain logic inside an application service
* you **should not** use an application service to send emails, push notifications etc. Use a port or a gateway instead
* an application service **should not** update the read model
{% endhint %}
