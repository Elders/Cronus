# Published Language

A **published language** is the shared vocabulary between two bounded contexts — the set of messages they agree on so one can consume what the other emits without coupling internal models. In Cronus the published language is defined by the messages you mark with `[DataContract]`.

## The DataContract convention

Every serialisable message, aggregate id, entity id, and value object in Cronus carries `[DataContract(Name = "<guid>")]`. The GUID is the **stable serialization id** for the type. It is not a version number and not a human-friendly name — it is the thing the wire format and the event store use to address a CLR type.

If you rename the class `OrderPlaced` to `OrderSubmitted`, the GUID in the attribute stays the same. Serialized events already on disk continue to resolve to the new class name, because look-up goes through the GUID, not through `typeof(T).FullName`.

`MessageInfo` implements this indirection:

{% code title="MessageInfo.cs (extract)" %}
```csharp
private static string GetAndCacheContractIdFromAttribute(Type contractType)
{
    DataContractAttribute contract = contractType
        .GetCustomAttributes(false).Where(attr => attr is DataContractAttribute)
        .SingleOrDefault() as DataContractAttribute;

    if (contract == null || String.IsNullOrEmpty(contract.Name))
    {
        throw new Exception(String.Format(
            "The message type '{0}' is missing a DataContract attribute. " +
            "Example: [DataContract(\"00000000-0000-0000-0000-000000000000\")]",
            contractType.FullName));
    }

    return contract.Name;
}
```
{% endcode %}

A type without `[DataContract(Name = ...)]` will throw the moment Cronus tries to serialise it — there is no fallback.

## The Namespace field is the bounded context

`DataContract.Namespace` doubles as the bounded context for the type:

{% code title="MessageInfo.cs (extract)" %}
```csharp
public static string GetBoundedContext(this Type messageType, string defaultBoundedContext = "implicit")
{
    // ... cached look-up ...
    if (contract is null == false && contract.IsNamespaceSetExplicitly)
        boundedContext = contract.Namespace;

    return boundedContext.ToLower();
}
```
{% endcode %}

If a type sets `Namespace` explicitly, that value wins; otherwise the default `"implicit"` is used. The value is always lower-cased, because RabbitMQ exchanges and queues are case-sensitive and Cronus keeps the routing keys canonical.

See [`bounded-context.md`](bounded-context.md) for how that value flows into RabbitMQ routing and Cassandra keyspaces.

## Real examples

The convention is uniform across the Elders ecosystem. A few live attributes:

* Framework-level: `Urn` and `AggregateRootId` both opt in.

```csharp
// Cronus.DomainModeling/Urn.cs
[DataContract(Name = "d3ff08b5-38e2-4aaf-b3a8-ccc423ed096d")]
public class Urn : IEquatable<Urn>, IBlobId { /* ... */ }

// Cronus.DomainModeling/AggregateRootId.cs
[DataContract(Name = "b78e63f3-1443-4e82-ba4c-9b12883518b9")]
public partial class AggregateRootId : Urn { /* ... */ }
```

* An event in a product code-base (no explicit namespace, so it routes under the service's own bounded context):

```csharp
// locus.backend/Experiences/Events/ExperienceMovedToBin.cs
[DataContract(Name = "945df1c4-ae46-4849-a094-e83a67abc95b")]
public class ExperienceMovedToBin : IEvent
{
    [DataMember(Order = 1)] public ExperienceId Id { get; private set; }
    [DataMember(Order = 2)] public UserId MovedBy { get; private set; }
    [DataMember(Order = 3)] public DateTime UpdatedAt { get; private set; }
}
```

* A contract shared across services — the namespace is set so the routing stays stable regardless of which host serialises it:

```csharp
// Elders.IdentityAndAccess.Contracts/Profiles/ProfileName.cs
[DataContract(Namespace = BC.IdentityAndAccess,
              Name = "6f353a50-34ec-47dc-ba9f-1862caff7010")]
public sealed record ProfileName
{
    [DataMember(Order = 1)] public string Name { get; init; }
    [DataMember(Order = 2)] public string FirstName { get; init; }
    /* ... */
}
```

{% hint style="info" %}
Cronus does not ship a `ValueObject<T>` base class. Use `record` for value-object semantics, or hand-write `Equals`/`GetHashCode` on a plain class. Older Cronus packages exposed a `ValueObject<T>` base — code you find in legacy services (e.g. older `Elders.IdentityAndAccess` or `locus.backend` branches) may still reference it.
{% endhint %}

Note the companion convention: a static constants class (`BC`) holds the bounded-context strings so each contract references the same literal.

## Why this matters for event sourcing

Event sourcing persists events forever. If a rename, a move, or a refactor broke the mapping between a serialized payload and a CLR type, replay would stop working. The GUID in `[DataContract(Name = ...)]` is the insurance against that: it pins the serialization id to the type across every rename and every namespace move.

`[DataMember(Order = N)]` on every persisted property is the field-level equivalent: removing or renaming a field without keeping its `Order` number stable breaks replay.

## Versioning discipline

When the shape of an event has to change in a way that is not backwards-compatible, do not mutate the existing type. Create a new type with a new GUID, then add a migration to transform old payloads into the new shape during replay.

{% hint style="success" %}
You **can** rename the CLR type, move it between namespaces, or change its members' CLR names — as long as `[DataContract(Name)]` and the `[DataMember(Order)]` values are preserved.

You **should** treat the `Name` GUID as effectively public API: once a message with that contract has been persisted or published, the GUID is burned in.

You **must** mint a new GUID for every semantically new message type. Never reuse an old GUID for a type with new meaning.
{% endhint %}

{% hint style="warning" %}
You **should not** change `[DataMember(Order = ...)]` values on an existing contract, and you **should not** delete persisted fields. Add a new contract with a new GUID and migrate instead.
{% endhint %}

## Related

{% content-ref url="bounded-context.md" %}
[bounded-context.md](bounded-context.md)
{% endcontent-ref %}

{% content-ref url="messages/events.md" %}
[events.md](messages/events.md)
{% endcontent-ref %}

{% content-ref url="messages/public-events.md" %}
[public-events.md](messages/public-events.md)
{% endcontent-ref %}
