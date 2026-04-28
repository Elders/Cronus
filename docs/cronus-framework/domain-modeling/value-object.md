# Value Object

A **value object** is an immutable, identity-less piece of the domain model. Two value objects are equal when their properties are equal — there is no "same-ness" tracked separately from the data itself. Prices, coordinates, phone numbers, date ranges, colour codes: these are all value objects. See Martin Fowler's [short piece](https://martinfowler.com/bliki/ValueObject.html) for the canonical definition.

{% hint style="warning" %}
Cronus does **not** ship a `ValueObject<T>` base class. Older versions of the library had one, but it has been removed. You are free to use any immutable type that supports value-based equality.
{% endhint %}

The two idiomatic options on modern .NET are records and hand-written immutable classes.

## Option 1 — record (recommended)

`record` types give you structural equality for free:

{% code title="Money.cs" %}
```csharp
[DataContract(Name = "1b6187f0-88c7-46d5-a22d-b39301765412")]
public record Money
{
    Money() { }

    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    [DataMember(Order = 1)] public decimal Amount { get; init; }
    [DataMember(Order = 2)] public string Currency { get; init; }

    public Money Add(Money other)
    {
        if (Currency != other.Currency) throw new InvalidOperationException("Cannot add different currencies.");
        return new Money(Amount + other.Amount, Currency);
    }
}
```
{% endcode %}

Two `Money(10, "EUR")` instances are equal by definition. `GetHashCode`, `Equals` and the `==` / `!=` operators are generated for you.

## Option 2 — immutable class with manual equality

If you need more control (base classes, private backing fields, bespoke comparisons) write a regular class:

{% code title="GeoCoordinate.cs" %}
```csharp
[DataContract(Name = "5d0b3b92-87bf-4f3c-b1a6-70de54d2cbfd")]
public sealed class GeoCoordinate : IEquatable<GeoCoordinate>
{
    GeoCoordinate() { }

    public GeoCoordinate(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90) throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude < -180 || longitude > 180) throw new ArgumentOutOfRangeException(nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
    }

    [DataMember(Order = 1)] public double Latitude { get; private set; }
    [DataMember(Order = 2)] public double Longitude { get; private set; }

    public bool Equals(GeoCoordinate other) =>
        other is not null && Latitude == other.Latitude && Longitude == other.Longitude;

    public override bool Equals(object obj) => Equals(obj as GeoCoordinate);

    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

    public static bool operator ==(GeoCoordinate left, GeoCoordinate right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(GeoCoordinate left, GeoCoordinate right) => !(left == right);
}
```
{% endcode %}

{% hint style="success" %}
**You can / should / must**

* a value object **must** be immutable once constructed
* a value object **must** validate its inputs in the constructor — an invalid value never exists
* a value object **should** carry behaviour that makes sense for the value (`Money.Add`, `DateRange.Overlaps`)
* a value object **must** override equality and `GetHashCode` (records do it for you)
* a value object **should** keep a parameterless constructor and `[DataContract]` attributes so it round-trips through serialization
{% endhint %}

## Collections of value objects

If a property is a collection of value objects, make sure the collection itself supports element-by-element equality. Standard `List<T>` does not — two `List<T>` with the same contents are not equal under default equality. Either use `HashSet<T>` with a value-equality comparer, or compute equality explicitly.

{% hint style="info" %}
If you ever find yourself writing "mutate" methods on a value object, step back: you probably want an _entity_ instead.
{% endhint %}

## Serialization reminder

Value objects travel inside events, commands and projection state, so they must serialize cleanly:

{% content-ref url="../messaging/serialization.md" %}
[serialization.md](../messaging/serialization.md)
{% endcontent-ref %}
