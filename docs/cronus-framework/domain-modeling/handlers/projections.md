# Projections

A projection is a representation of an object using a different perspective. In the context of CQRS, projections are queriable models on the "_read_" side that never manipulate the original data \(events in event-sourced systems\) in any way. Projections should be designed in a way that is useful and convenient for the reader \(API, UI, etc.\).

Cronus supports non-event-sourced and event-sourced projections with snapshots.

## Defining a projection

To create a projection, create a class for it that inherits `ProjectionDefinition<TState, TId>`. The id can be any type that implements the `IBlobId` interface. All ids provided by Cronus implement this interface but it is common to create your own for specific business cases. The `ProjectionDefinition<TState, TId>` base class provides a `Subscribe()` method that is used to create a projection id from an event. This will define an event-sourced projection with a state that will be used to persist snapshots.

Use the `IEventHandler<TEvent>` interface to indicate that the projection can handle events of the specified event type. Implement this interface for each event type your projection needs to handle.

```csharp
// TODO: give a relevant example
[DataContract(Name = "bae8bd10-9903-4960-95c4-b4fa4688a860")]
public class ExampleByIdProjection : ProjectionDefinition<ExampleByIdProjectionState, ExampleId>,
    IEventHandler<ExampleCreated>
{
		public ExampleByIdProjection()
		{
		    Subscribe<ExampleCreated>(x => x.Id);
		}

		public void Handle(ExampleCreated @event)
		{
		    State.Id = @event.Id;
		    State.Name = @event.Name;
		}
}
```

Create a class for the projection state. The state of the projection gets serialized and deserialized when persisting or restoring a snapshot. That's why it must have a parameterless constructor, a data contract and data members.

{% page-ref page="../../serialization.md" %}

```csharp
// TODO: give a relevant example
[DataContract(Name = "ed879ae7-e238-43eb-99f0-3a39c6c935e0")]
public class ExampleByIdProjectionState
{
    [DataMember(Order = 1)]
    public ExampleId Id { get; set; }

    [DataMember(Order = 2)]
    public ExampleName Name { get; set; }
}
```

{% hint style="info" %}
There is no guarantee the events will be handled in the order of publishing nor that every event will be handled at most once. That's why you should design projections in a way that solves those problems. Always assign all possible properties from the handled event to the state and make sure the projection is idempotent.
{% endhint %}

{% hint style="info" %}
If the projection state contains a collection, make sure it doesn't get populated with duplicates. This can be achieved by using a `HashSet<T>` and `ValueObject`.
{% endhint %}

You can define a non-event-sourced projection by decorating it with the `IProjection` interface. This is useful when you want to persist the state in an external system \(e.g. ElasticSearch, relational database\).

```csharp
// TODO: give a relevant example
[DataContract(Name = "af157a4d-7608-4c9d-8e42-63bd483a8ad4")]
public class ExampleEfProjection : IProjection,
        IEventHandler<ExampleCreated>
{
		public DbContext Context { get; set; }

		public void Handle(ExampleCreated @event)
    {
				var exampleDto = new ExampleDto(@event.Id, @event.Name);
        Context.Examples.Add(exampleDto);
        Context.SaveChanges();
    }
}
```

By default, all projections' states are being persisted as snapshots. If you want to disable this feature for a specific projection, use the `IAmNotSnapshotable` interface.

```csharp
// TODO: give a relevant example
[DataContract(Name = "bae8bd10-9903-4960-95c4-b4fa4688a860")]
public class ExampleByIdProjection : ProjectionDefinition<ExampleByIdProjectionState, ExampleId>,
    IEventHandler<ExampleCreated>,
    IAmNotSnapshotable
{
		// ...
}
```

## Querying a projection

To query a projection, you need to inject an instance of `IProjectionReader` in your code and invoke the `Get()` or `GetAsync()` method. The returned object will be of type `ReadResult` or `Task<ReadResult>` containing the projection and a few properties indicating if the loading was successful.

```csharp
public class GetExampleController : ControllerBase
{
    private IProjectionReader projectionReader;
    
    public GetExampleController(IProjectionReader projectionReader)
    {
        this.projectionReader = projectionReader;
    }

    public async Task<IActionResult> GetExample(GetExampleRequest request)
    {
				var id = ExampleId.New(request.Tenant, request.Id);
        var result = await projectionReader.GetAsync<ExampleByIdProjection>(id);
        if (result.IsSuccess)
            return Ok(new GetExampleResponse(result.Data.State));
        else
            return BadRequest(result.Error);
    }

		public class GetExampleResponse
		{
				// ...
		}
}
```

{% hint style="info" %}
Use separate models for the API responses from the projection states to ensure you won't introduce breaking changes if the projection gets modified.
{% endhint %}

## Projection versioning

TODO

## Best Practices

{% hint style="success" %}
**You can/should/must...**

* a projection **must** be idempotent
* a projection **must not** issue new commands or events
{% endhint %}

{% hint style="warning" %}
**You should not...**

* a projection **should not** query other projections. All the data of a projection must be collected from the Events' data
{% endhint %}

