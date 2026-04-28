# IDs

Identity in Cronus is built on URNs. Every aggregate root, every entity, every projection, and every message that carries an id uses a URN-shaped value object. This page walks through the concrete types you will use, what they encode, and how to derive your own.

## The URN foundation

The base class is `Urn` (from `Elders.Cronus.DomainModeling`):

{% code title="Urn.cs" %}
```csharp
[DataContract(Name = "d3ff08b5-38e2-4aaf-b3a8-ccc423ed096d")]
public class Urn : IEquatable<Urn>, IBlobId
{
    public const char PARTS_DELIMITER = ':';
    public const char HIERARCHICAL_DELIMITER = '/';
    public const string UriSchemeUrn = "urn";
    // ...
}
```
{% endcode %}

A URN has three interesting parts for our purposes:

* the scheme, always `urn`,
* the NID (namespace identifier) — in Cronus, the **tenant**,
* the NSS (namespace-specific string) — everything that identifies the resource inside the tenant.

Cronus also accepts r-, q-, and f-components per [RFC 8141](https://tools.ietf.org/html/rfc8141), and by default is case-insensitive (`Urn.UseCaseSensitiveUrns = false`). NIDs must be 2–32 characters, alphanumeric plus `-`, and may not start or end with `-` nor contain the string `urn`.

## AggregateRootId

`AggregateRootId` is the concrete URN used for aggregate roots. Its NSS is `arname:id`, so a full aggregate id looks like:

```
urn:acme:order:a2c19b5f-0d2e-45f3-81d1-7a5b6c9d4ee8
    │    │     └─ the per-tenant id part
    │    └─ the aggregate-root name
    └─ the tenant (NID)
```

The load-bearing constructor takes those three pieces in order:

{% code title="AggregateRootId.cs" %}
```csharp
[DataContract(Name = "b78e63f3-1443-4e82-ba4c-9b12883518b9")]
public partial class AggregateRootId : Urn
{
    public AggregateRootId(string tenant, string arName, string id)
        : base(tenant, $"{arName}{PARTS_DELIMITER}{id}")
    { /* ... */ }

    public string Id { get; }
    public string Tenant { get; }
    public string AggregateRootName { get; }
}
```
{% endcode %}

Argument order is **`tenant, arName, id`** — a frequent source of copy-paste bugs. Three companion statics make parsing easy:

```csharp
AggregateRootId parsed  = AggregateRootId.Parse("urn:acme:order:a2c1...");
bool ok = AggregateRootId.TryParse("urn:acme:order:a2c1...", out AggregateRootId id);
```

`Parse` throws `ArgumentException` on an invalid URN; `TryParse` returns `false`.

## Deriving a typed aggregate id

In practice you never pass around a raw `AggregateRootId`. You derive a named subclass per aggregate, decorate it with a stable `[DataContract(Name = "<guid>")]`, and forward the three constructor arguments:

{% code title="ExperienceId.cs (grounded in locus.backend)" %}
```csharp
[DataContract(Name = "1e8bf099-2cca-4760-bfbd-f029ca02d359")]
public class ExperienceId : AggregateRootId
{
    protected ExperienceId() { }

    public ExperienceId(string id, string tenant)
        : base(tenant, "experience", id) { }

    public ExperienceId(string tenant)
        : this(Guid.NewGuid().ToString(), tenant) { }
}
```
{% endcode %}

The private parameterless constructor is required for deserialisation. The aggregate-root name (`"experience"`) is the only free-form part — keep it lower-case, short, and stable. Changing it later is a store-fork event.

## EntityId

For entities hanging off an aggregate root, Cronus provides `EntityId<TAggregateRootId>`:

{% code title="EntityId.cs" %}
```csharp
public abstract class EntityId<TAggregateRootId> : EntityId
    where TAggregateRootId : AggregateRootId
{
    protected EntityId() { }

    public EntityId(ReadOnlySpan<char> idBase, TAggregateRootId rootId) { /* ... */ }

    protected abstract ReadOnlySpan<char> EntityName { get; }

    public TAggregateRootId AggregateRootId { get; }
}
```
{% endcode %}

Deriving an entity id looks like this:

```csharp
[DataContract(Name = "1d23c591-219f-491e-bfb1-a775fe2751b6")]
public class WalletId : EntityId<UserId>
{
    protected override ReadOnlySpan<char> EntityName => "wallet";

    WalletId() { }

    public WalletId(string idBase, UserId rootId) : base(idBase.AsSpan(), rootId) { }
}
```

The NSS becomes `arname:arid/entityname:entityid`, so a full entity URN reads:

```
urn:acme:user:u-001/wallet:main
```

The hierarchical `/` separates the aggregate from the entity, the `:` separates name from id on each side. This is what `EntityId.EntityRegex()` validates against.

## What does NOT exist

Several names you may see in older code, blog posts, or scratch branches are **not** part of the current framework:

* `AggregateRootId<T>` — a generic base class was sketched but left commented-out inside `AggregateRootId.cs`; there is no such generic today. Derive from the non-generic `AggregateRootId` instead.
* `AggregateUrn` — not defined anywhere in `Elders.Cronus.DomainModeling`. The previous separate type was collapsed into `AggregateRootId`.
* `IUrn` — no such interface. `Urn` implements `IBlobId`.
* `IAggregateRootId<T>` — no generic form of `IAggregateRootId` exists in master.
* `StringTenantId` — not in the current sources. Legacy sample code that references it is stale.

If you are porting older code, delete the references and lean on `AggregateRootId` directly.

## Guidelines

{% hint style="success" %}
You **can** derive as many id types as you need — one per aggregate root and one per entity.

You **should** give every id type a stable `[DataContract(Name = "<guid>")]` and a `protected` or `private` parameterless constructor.

You **must** pass the constructor arguments in the order `(tenant, arName, id)` for `AggregateRootId`. Mixing them up will produce URNs that look valid but cannot be re-parsed against the aggregate-root-name regex.
{% endhint %}

{% hint style="warning" %}
You **should not** encode mutable data (emails, display names, tenant-friendly slugs) into the NSS. Ids are forever; only stable keys belong there.
{% endhint %}

## Related

{% content-ref url="multitenancy.md" %}
[multitenancy.md](multitenancy.md)
{% endcontent-ref %}

{% content-ref url="aggregate.md" %}
[aggregate.md](aggregate.md)
{% endcontent-ref %}

{% content-ref url="entity.md" %}
[entity.md](entity.md)
{% endcontent-ref %}
