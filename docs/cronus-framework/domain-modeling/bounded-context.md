# Bounded Context

In Domain-Driven Design a **bounded context** is the explicit boundary inside which a model is consistent and unambiguous. Two teams working on the same company may use the word "Order" to mean different things — DDD accepts that and asks you to name the context each meaning lives in. For the conceptual foundation, see [`concepts/ddd.md`](../concepts/ddd.md).

Cronus turns that concept into a single, load-bearing configuration value: `Cronus:BoundedContext`.

## One name, many side effects

The `Cronus:BoundedContext` setting is **one alphanumeric string**. It prefixes:

* every RabbitMQ exchange and queue the host declares,
* every Cassandra keyspace the event store and projection store write to,
* the default `Namespace` stored against contracts whose type does not set one explicitly.

Change that string on a running system and you fork your whole store: new queues, new keyspaces, and the old streams become invisible.

## The POCO

The setting is bound to the `BoundedContext` options class:

{% code title="Elders.Cronus/BoundedContext.cs" %}
```csharp
public class BoundedContext
{
    [Required(AllowEmptyStrings = false,
        ErrorMessage = "The configuration `Cronus:BoundedContext` is required.")]
    [RegularExpression(@"^\b([\w\d_]+$)",
        ErrorMessage = "Characters are not allowed for configuration `Cronus:BoundedContext`.")]
    public string Name { get; set; }

    public override string ToString() => Name;
}

public class BoundedContextProvider : CronusOptionsProviderBase<BoundedContext>
{
    public const string SettingKey = "cronus:boundedcontext";

    public override void Configure(BoundedContext options)
    {
        options.Name = configuration[SettingKey]?.ToLower()?.Trim();
    }
}
```
{% endcode %}

Two rules are enforced by the class itself:

* The value is **required** — a host will not start without it.
* The value must match `^\b([\w\d_]+$)`: alphanumeric characters and underscores only. No dots, dashes, or spaces.
* `BoundedContextProvider` lower-cases and trims the input, which matches RabbitMQ's case-sensitivity and keeps names canonical.

Inject `IOptionsMonitor<BoundedContext>` to read the current value at runtime.

{% code title="appsettings.json" %}
```json
{
  "cronus": {
    "boundedcontext": "billing",
    "tenants": [ "acme" ]
  }
}
```
{% endcode %}

## Bounded context on the wire: the `Namespace` field

Every message type in Cronus carries a `[DataContract]` attribute. The `Name` parameter is the contract id (a GUID). The `Namespace` parameter is the bounded context the type belongs to.

{% code title="MessageInfo.cs (extract)" %}
```csharp
public static string GetBoundedContext(this Type messageType, string defaultBoundedContext = "implicit")
{
    string boundedContext;
    if (!typeToBoundedContext.TryGetValue(messageType, out boundedContext))
    {
        boundedContext = GetAndCacheBoundedContextFromAttribute(messageType, defaultBoundedContext);
    }
    return boundedContext;
}
```
{% endcode %}

If the type sets `Namespace` explicitly, that wins; otherwise the caller's default is used. For a service's own contracts the default is `"implicit"`, meaning "route under this service's own bounded context". For contracts shared with other services, set `Namespace` explicitly so routing stays stable regardless of which host serializes the message.

A common pattern is to keep a static constants class in your `.Contracts` project:

{% code title="Elders.IdentityAndAccess.Contracts/BC.cs" %}
```csharp
public static class BC
{
    public const string IdentityAndAccess = "IdentityAndAccess";
}
```
{% endcode %}

and reference it from every contract:

```csharp
[DataContract(Namespace = BC.IdentityAndAccess, Name = "73ffd67d-b775-4e53-ac87-90de404fc58a")]
public class TenantId : AggregateRootId { /* ... */ }
```

That keeps the bounded-context string in exactly one place per contract assembly.

## Choosing a name

{% hint style="success" %}
You **can** name your bounded context after the business capability it encapsulates (`billing`, `identityandaccess`, `catalog`).

You **should** keep the name short — it is concatenated into every RabbitMQ routing key and Cassandra keyspace name in the system.

You **must** choose the name before the first production message flows through the service. Changing `Cronus:BoundedContext` later will orphan every existing stream, queue, and projection table.
{% endhint %}

{% hint style="warning" %}
You **should not** include the environment (`billing_prod`, `billing_dev`) in the bounded-context name. Use separate configuration files or deployment environments instead — the bounded context is part of the domain, not the deployment topology.
{% endhint %}

## Related

{% content-ref url="../configuration.md" %}
[configuration.md](../configuration.md)
{% endcontent-ref %}

{% content-ref url="published-language.md" %}
[published-language.md](published-language.md)
{% endcontent-ref %}

{% content-ref url="../concepts/ddd.md" %}
[ddd.md](../concepts/ddd.md)
{% endcontent-ref %}
