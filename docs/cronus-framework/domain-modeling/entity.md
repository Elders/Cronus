# Entity

An entity is an object that has an identity and is mutable. Each entity is uniquely identified by an ID rather than by its properties; therefore, two entities can be considered equal if both of them have the same ID even though they have different properties.

You can define an entity with Cronus using the `Entity<TAggregateRoot, TEntityState>` base class. To publish an event from an entity, use the `Apply(IEvent @event)` method provided by the base class.

```csharp
public class Wallet : Entity<UserAggregate, WalletState>
{
    public Wallet(UserAggregate root, WalletId entityId, string name, decimal amount) : base(root, entityId)
    {
        state.EntityId = entityId;
        state.Name = name;
        state.Amount = amount;
    }

    public void AddMoney(decimal value, UserId userId)
    {

        if (value > 0)
        {
            IEvent @event = new AddMoney(state.EntityId, userId, value, DateTimeOffset.UtcNow);
            Apply(@event);
        }
    }
}
```

{% hint style="info" %}
Set the initial state of the entity using the constructor. The event responsible for creating the entity is being published by the root/parent to modify its state. That means that you can not \(and should not\) subscribe to that event in the entity state using `When(Event e)`.
{% endhint %}

## Entity state

The entity state keeps current data of the entity and is responsible for changing it based on events raised only by the same entity.

Use the abstract helper class `EntityState<TEntityId>` to create an entity state. It can be accessed in the entity using the `state`field provided by the base class. Also, you can implement the `IEntityState` interface by yourself in case inheritance is not a viable option.

To change the state of an entity, create event-handler methods for each event with a method signature `public void When(Event e) { ... }`.

```csharp
public class WalletState : EntityState<WalletId>
{
    public override WalletId EntityId { get; set; }

    public string Name { get; set; }

    public decimal Amount { get; set; }
}
```

## Entity id

All entity ids must implement the `IEntityId` interface. Since Cronus uses [URNs](https://en.wikipedia.org/wiki/Uniform_Resource_Name) for ids that will require implementing the [URN specification](https://tools.ietf.org/html/rfc8141) as well. If you don't want to do that, you can use the provided helper base class `EntityId<TAggregateRootId>`.

```csharp
[DataContract(Name = "1d23c591-219f-491e-bfb1-a775fe2751b6")]
public class WalletId : EntityId<UserId>
{
    protected override ReadOnlySpan<char> EntityName => "wallet";

    WalletId() { }

    public WalletId(string id, UserId idBase) : base(id.AsSpan(), idBase) { }
}
```

