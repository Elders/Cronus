# Value Object

 A Value Object is an immutable type that is distinguishable only by the state of its properties. That is, unlike an [Entity](https://deviq.com/entity/), which has a unique identifier and remains distinct even if its properties are otherwise identical, two Value Objects with the exact same properties can be considered equal. Value Objects are a pattern first described in Evans’ [Domain-Driven Design book](https://amzn.to/1Lkgs7B) and further explained in Martin Fowler’s [Value Object article](https://martinfowler.com/bliki/ValueObject.html).



```csharp
[DataContract(Name = "1b6187f0-88c7-46d5-a22d-b39301765412")]
public class Performer: ValueObject<Performer>
{
    Performer(){}

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

