# Value Object

Value objects represent immutable and atomic data. They are distinguishable only by the state of their properties and do not have an identity or any identity tracking mechanism. Two value objects with the exact same properties can be considered equal. You can read more about value objects in [this](https://martinfowler.com/bliki/ValueObject.html) article.

To define a value object with Cronus, create a class that inherits the base helper class `ValueObject<T>`. Keep all related to the value object business rules and data within the class.

```csharp
[DataContract(Name = "1b6187f0-88c7-46d5-a22d-b39301765412")]
public class Performer: ValueObject<Performer>
{
    Performer() {}

    public Performer(string name, string coverImage)
    {
        // null check
        Name = name;
        CoverImage = coverImage;
    }

    [DataMember(Order = 1)]
    public string Name { get; private set; }

    [DataMember(Order = 2)]
    public string CoverImage { get; private set; }
}
```

The base class `ValueObject<T>` implements the `IEqualityComparer<T>` and `IEquatable<T>` interfaces. When comparing two value objects of the same type the properties from the first are being compared with the properties of the second using reflection. The base class also overrides the `==` and `!=` operators.

{% hint style="info" %}
If a value object contains a collection of items, make sure that the items are also value objects and the collection supports item-by-item comparison. Otherwise, you will have to override the default comparison algorithm.
{% endhint %}

Keep a parameterless constructor and specify a data contract for serialization.

{% page-ref page="../messaging/serialization.md" %}

