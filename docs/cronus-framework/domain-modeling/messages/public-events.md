# Public Events

A **public event** is an event you explicitly choose to share with the outside world — other bounded contexts, integration consumers, downstream services. It is the Cronus expression of a _published language_: a deliberate, versioned contract that outlives the internal representation of your domain events.

A public event implements `IPublicEvent`, which adds a `Tenant` property on top of `IMessage`:

```csharp
public interface IPublicEvent : IMessage
{
    string Tenant { get; }
}
```

{% hint style="info" %}
Public events are delivered over a separate transport exchange (for example `PublicRabbitMQ` in `appsettings.json`). Consumers in other services subscribe to them through their own Cronus host.
{% endhint %}

{% hint style="success" %}
**You can / should / must**

* a public event **must** be immutable and forward-compatible — once published it lives forever
* a public event **must** carry the `Tenant` that produced it
* a public event **should** contain enough context to be consumed without round-tripping back to the source
* you **should** keep public events thinner than internal events — only what the outside world actually needs
* you **must not** change existing `[DataContract(Name)]` GUIDs or property `Order` values
{% endhint %}

## Defining a public event

{% code title="AccountSuspended_public.cs" %}
```csharp
[DataContract(Name = "c6d9d1ae-5e54-4e1e-9121-9ab0c4f3f7a5")]
public class AccountSuspended_public : IPublicEvent
{
    AccountSuspended_public() { }

    public AccountSuspended_public(string tenant, string accountUrn, DateTimeOffset timestamp)
    {
        Tenant = tenant;
        AccountUrn = accountUrn;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)] public string Tenant { get; private set; }
    [DataMember(Order = 2)] public string AccountUrn { get; private set; }
    [DataMember(Order = 3)] public DateTimeOffset Timestamp { get; private set; }

    public override string ToString() => $"Account {AccountUrn} suspended in tenant {Tenant}.";
}
```
{% endcode %}

{% hint style="warning" %}
Do not reuse internal domain IDs directly as wire payloads. Convert them to their string URN (`id.Value`) before putting them on a public event; the receiving service will not share your assembly types.
{% endhint %}

## Emitting a public event

Public events are emitted from the aggregate root via an overload of `Apply`:

```csharp
public void Suspend()
{
    if (state.IsSuspended) return;

    Apply(new AccountSuspended(state.Id, DateTimeOffset.UtcNow));
    Apply(new AccountSuspended_public(state.Id.Tenant, state.Id.Value, DateTimeOffset.UtcNow));
}
```

Cronus tracks uncommitted public events separately from domain events (see `IUnderstandPublishedLanguage.UncommittedPublicEvents`). They are published **after** the aggregate commit has been persisted, so the outside world only ever sees events that actually happened.

## Handling a public event

Use `IPublicEventHandler<T>` when another bounded context needs to react to the published language:

```csharp
public class OnAccountSuspendedPort : IPort,
    IPublicEventHandler<AccountSuspended_public>
{
    public Task HandleAsync(AccountSuspended_public @event)
    {
        // translate external fact into a local command / projection update
        return Task.CompletedTask;
    }
}
```

{% content-ref url="../published-language.md" %}
[published-language.md](../published-language.md)
{% endcontent-ref %}
