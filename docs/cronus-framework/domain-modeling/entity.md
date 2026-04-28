# Entity

An **entity** is a domain object with identity. Two entities are equal when their IDs match, regardless of their other properties. In Cronus, an entity lives inside an aggregate and shares the aggregate's consistency boundary — the aggregate root is the only thing that mutates entities directly.

Use entities when a part of the aggregate needs its own identity-based lifecycle, but that lifecycle is still owned by the root. Common examples are versions inside a document, line items in an order, or rooms inside a building.

## Defining an entity

Inherit `Entity<TAggregateRoot, TEntityState>` and pass the root plus the entity ID into the base constructor. Events are emitted via the protected `Apply(IEvent)` method, just like in the aggregate root — but the event is wrapped in an `EntityEvent` so the right entity's state handler receives it.

{% code title="Wallet.cs" %}
```csharp
public class Wallet : Entity<UserAggregate, WalletState>
{
    public Wallet(UserAggregate root, WalletId entityId, string name, decimal openingBalance) : base(root, entityId)
    {
        Apply(new WalletOpened(entityId, name, openingBalance, DateTimeOffset.UtcNow));
    }

    public void AddMoney(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Apply(new MoneyAdded(state.EntityId, amount, DateTimeOffset.UtcNow));
    }
}
```
{% endcode %}

{% hint style="info" %}
The entity's creation event is emitted by the **root** (not by the entity itself). The root's state handles the "entity created" event to add the new entity to its collection. Inside the entity, use `When(TEvent)` on the entity state only for events that modify the entity _after_ creation.
{% endhint %}

## Entity state

Inherit `EntityState<TEntityId>`. Like the aggregate state, the entity state folds its events into itself via `public void When(TEvent e)` handlers.

{% code title="WalletState.cs" %}
```csharp
public class WalletState : EntityState<WalletId>
{
    public override WalletId EntityId { get; set; }
    public string Name { get; set; }
    public decimal Balance { get; set; }

    public void When(WalletOpened e)
    {
        EntityId = e.EntityId;
        Name = e.Name;
        Balance = e.OpeningBalance;
    }

    public void When(MoneyAdded e) => Balance += e.Amount;
}
```
{% endcode %}

## Entity ID

Inherit the generic `EntityId<TAggregateRootId>`. The base ctor takes the ID base plus the parent aggregate's ID, and you must override `EntityName`:

```csharp
public abstract class EntityId<TAggregateRootId> : EntityId
    where TAggregateRootId : AggregateRootId
{
    public EntityId(ReadOnlySpan<char> idBase, TAggregateRootId rootId);

    protected abstract ReadOnlySpan<char> EntityName { get; }
}
```

{% code title="WalletId.cs" %}
```csharp
[DataContract(Name = "1d23c591-219f-491e-bfb1-a775fe2751b6")]
public class WalletId : EntityId<UserId>
{
    WalletId() { }

    public WalletId(string id, UserId rootId) : base(id.AsSpan(), rootId) { }

    protected override ReadOnlySpan<char> EntityName => "wallet";
}
```
{% endcode %}

The URN of a wallet then looks like: `urn:tenant:user:<user-id>/wallet:<wallet-id>` — you can see the entity name appears after the hierarchical delimiter `/`.

{% hint style="success" %}
**You can / should / must**

* an entity **must** live inside exactly one aggregate
* an entity **must not** be loaded or saved independently — the aggregate owns the lifecycle
* an entity **should** have its own behaviour methods; moving logic to the root defeats the purpose
* an entity ID **must** include its parent aggregate's URN so the entity cannot be confused across aggregates
{% endhint %}

## Wiring the entity into the aggregate

The aggregate's state reacts to the "entity created" event, constructs the entity, and adds it to its collection. From that point on, the root can route calls to the entity's methods.

```csharp
public class UserAggregate : AggregateRoot<UserState>
{
    public void OpenWallet(WalletId walletId, string name, decimal openingBalance)
    {
        new Wallet(this, walletId, name, openingBalance); // ctor applies WalletOpened
    }

    public void AddToWallet(WalletId walletId, decimal amount)
    {
        var wallet = state.Wallets.First(w => w.EntityId.Equals(walletId));
        // In a real aggregate, state exposes wallets as live entities, not just state rows.
        // See the handlers pages for the full pattern.
    }
}
```

See the aggregate page for the companion pieces (root, state, ID):

{% content-ref url="aggregate.md" %}
[aggregate.md](aggregate.md)
{% endcontent-ref %}
