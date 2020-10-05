# Aggregate

Aggregates represent the business models explicitly. They are designed to fully match any needed requirements. Any change done to an instance of an aggregate goes through the aggregate root.

### Aggregate root

Creating an aggregate root with Cronus is as simple as writing a class that inherits`AggregateRoot<TState>` and a class for the state of the aggregate root.

```csharp
public class Concert : AggregateRoot<ConcertState>
{
    Concert() {} // keep the private parameterless constructor
    
    public Concert(string name, Venue venue, DateTimeOffset startTime, TimeSpan duration)
    {
        // business logic for creating a concert
    }

    public void RegisterPerformer(Performer performer)
    {
        // business logic for registering a performer
    }
    
    // ...
}
```

### Aggregate root state

The aggregate root state keeps current data of the aggregate root and is responsible for changing it based on events raised only by the root.

Use the abstract helper class `AggregateRootState<TAggregateRoot, TAggregateRootId>` to create an aggregate root state. It can be accessed in the aggregate root using the `state` field provided by the base class. Also, you can implement the `IAggregateRootState` interface by yourself in case inheritance is not a viable option.

To change the state of an aggregate root, create event-handler methods for each event with a method signature `public void When(Event e) { ... }`.

```csharp
public class ConcertState : AggregateRootState<Concert, ConcertId>
{
    public ConcertState()
    {
        Performers = new List<Performer>();
    }

    public override ConcertId Id { get; set; }

    public string Name { get; private set; }

    public Venue Venue { get; private set; }

    public DateTimeOffset StartTime { get; private set; }

    public TimeSpan Duration { get; private set; }

    public List<Performer> Performers { get; private set; }
    
    public void When(ConcertAnnounced @event)
    {
        // change the state here ...
    }
}
```

### Aggregate root id

All aggregate root ids must implement the `IAggregateRootId` interface. Since Cronus uses [URNs](https://en.wikipedia.org/wiki/Uniform_Resource_Name) for ids that will require implementing the [URN specification](https://tools.ietf.org/html/rfc8141) as well. If you don't want to do that, you can use the provided helper base class `AggregateRootId`.

```csharp
public class ConcertId : AggregateRootId
{
    const string RootName = "concert";

    public ConcertId(AggregateUrn urn) : base(RootName, urn) { }
    public ConcertId(string idBase, string tenant) : base(idBase, RootName, tenant) { }
    protected ConcertId() { }
}
```

Another option is to use the `AggregateRootId<T>` class. This will give you more flexibility in constructing instances of the id. Also, parsing URNs will return the specified type `T` instead of `AggregateUrn`.

```csharp
public class ConcertId : AggregateRootId<ConcertId>
{
    const string RootName = "concert";

    ConcertId() { }
    public ConcertId(string id, string tenant) : base(id, RootName, tenant) { }

    protected override ConcertId Construct(string id, string tenant)
    {
        return new ConcertId(id, tenant);
    }
}
```

