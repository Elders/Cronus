# Aggregate

Aggregates represent the business models explicitly. They are designed to fully match any needed requirements. Any change done to an instance of an aggregate goes through the aggregate root.

### Aggregate root

Creating an aggregate root with Cronus is as simple as writing a class that inherits `AggregateRoot<TState>` and a class for the state of the aggregate root.

```csharp
public class Concert : AggregateRoot<ConcertState>
{
    Concert() {} // keep the private constructor
    
    public Concert(string name, Venue venue, DateTimeOffset startTime, TimeSpan duration)
    {
        // ...
    }

    public void RegisterPerformer(Performer performer)
    {
        // ...
    }
    
    // ...
}
```

### Aggregate root state

Use the abstract helper class `AggregateRootState<TAggregateRoot, TAggregateRootId>`. Also, you can implement the `IAggregateRootState` by yourself in case inheriting is not a viable option.

To change the state of an aggregate root, define event handlers for each event with the method signature `public void When(Event e) { ... }`.

```csharp
public class ConcertState : AggregateRootState<Concert, ConcertId>
{
    public ConcertState()
    {
        Performers = new List<Performer>();
    }

    public override ConcertId Id { get; set; }

    public string Name { get; set; }

    public Venue Venue { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public TimeSpan Duration { get; set; }

    public List<Performer> Performers { get; set; }
    
    public When(ConcertAnnounced @event)
    {
        // ...
    }
}
```

### Aggregate root id

All aggregate root ids must implement the `IAggregateRootId` interface. Since Cronus uses URNs for ids that will require implementing the URN specification as well. If you don't want to do that, you can use the provided helper base classes `AggregateRootId`, `AggregateUrn` or `Urn`. The example above uses `AggregateRootId` because it inherits the other two and it is the easiest to implement.

```csharp
public class ConcertId : AggregateRootId
{
    const string RootName = "concert";

    public ConcertId(AggregateUrn urn) : base(RootName, urn) { }
    public ConcertId(string idBase, string tenant) : base(idBase, RootName, tenant) { }
    protected ConcertId() { }
}
```

