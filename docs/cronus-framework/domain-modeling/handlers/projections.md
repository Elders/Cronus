# Projections

A **projection** is a read model. It is a derived representation of the events in the event store, shaped for the way a particular reader wants to consume data — an API endpoint, a dashboard, a search index. A projection never mutates the source of truth; it only applies events to its own state.

Cronus supports two flavours:

* **Event-sourced projections** — inherit `ProjectionDefinition<TState, TId>` and subscribe to the events they care about. Their state is rebuilt by replaying events, optionally from a stored snapshot.
* **Non-event-sourced projections** — implement `IProjection` directly and persist state to an external store (for example Elasticsearch or a relational database).

## Defining an event-sourced projection

Inherit `ProjectionDefinition<TState, TId>` where `TState` is a serializable plain class and `TId` is any type that implements `IBlobId`. Declare `IEventHandler<TEvent>` once per event you want to handle. In the constructor, call `Subscribe<TEvent>(event => projectionId)` for every event — that's how Cronus knows which projection instance a given event belongs to.

{% code title="TaskProjection.cs" %}
```csharp
[DataContract(Name = "c94513d1-e5ee-4aae-8c0f-6e85b63a4e03")]
public class TaskProjection : ProjectionDefinition<TaskProjectionState, UserId>,
    IEventHandler<TaskCreated>,
    IEventHandler<TaskClosed>
{
    public TaskProjection()
    {
        Subscribe<TaskCreated>(e => e.UserId);
        Subscribe<TaskClosed>(e => e.UserId);
    }

    public Task HandleAsync(TaskCreated @event)
    {
        State.Tasks.Add(new TaskProjectionState.Entry
        {
            Id = @event.Id,
            Name = @event.Name,
            CreatedAt = @event.Timestamp,
            IsClosed = false
        });
        return Task.CompletedTask;
    }

    public Task HandleAsync(TaskClosed @event)
    {
        var entry = State.Tasks.FirstOrDefault(x => x.Id.Equals(@event.Id));
        if (entry is not null) entry.IsClosed = true;
        return Task.CompletedTask;
    }

    public IEnumerable<TaskProjectionState.Entry> TasksByName(string name)
        => State.Tasks.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
```
{% endcode %}

{% code title="TaskProjectionState.cs" %}
```csharp
[DataContract(Name = "c135893e-b9e3-453a-b0e0-53545094ec5d")]
public class TaskProjectionState
{
    public TaskProjectionState() { Tasks = new List<Entry>(); }

    [DataMember(Order = 1)]
    public List<Entry> Tasks { get; set; }

    [DataContract(Name = "317b3cbb-593a-4ffc-8284-d5f5c599d8ae")]
    public class Entry
    {
        [DataMember(Order = 1)] public TaskId Id { get; set; }
        [DataMember(Order = 2)] public string Name { get; set; }
        [DataMember(Order = 3)] public DateTimeOffset CreatedAt { get; set; }
        [DataMember(Order = 4)] public bool IsClosed { get; set; }
    }
}
```
{% endcode %}

The state class is serialised into a snapshot, so it needs a parameterless constructor, a `[DataContract]` attribute, and `[DataMember(Order = n)]` on every persisted property — the same rules as for events.

{% content-ref url="../../messaging/serialization.md" %}
[serialization.md](../../messaging/serialization.md)
{% endcontent-ref %}

{% hint style="info" %}
`Subscribe<TEvent>(...)` returns the projection ID for the given event. One event can be routed to many projection IDs (return an array via chained calls), and one projection ID can aggregate many events.
{% endhint %}

## Idempotency

Cronus does not guarantee that an event is delivered exactly once, and out-of-order delivery can happen during catch-up or rebuild. Design your projection accordingly:

{% hint style="success" %}
**You can / should / must**

* a projection **must** be idempotent — applying the same event twice must not produce different state
* a projection **should** store every piece of data an event carries so rebuilding does not require another query
* a projection **must not** issue new commands or publish new events
* you **should** guard against duplicate entries in collections (e.g. a `HashSet<T>` with value-equality)
{% endhint %}

{% hint style="warning" %}
**You should not**

* a projection **should not** query other projections — derive everything from the events it subscribes to
* a projection **should not** perform external I/O inside `HandleAsync`
{% endhint %}

## Non-event-sourced projection

Sometimes the read model lives in an external store that tracks its own consistency — a SQL database via EF Core, for example. Implement `IProjection` directly and skip the snapshot machinery:

```csharp
[DataContract(Name = "af157a4d-7608-4c9d-8e42-63bd483a8ad4")]
public class TaskSqlProjection : IProjection,
    IEventHandler<TaskCreated>
{
    private readonly TaskDbContext db;

    public TaskSqlProjection(TaskDbContext db)
    {
        this.db = db;
    }

    public async Task HandleAsync(TaskCreated @event)
    {
        db.Tasks.Add(new TaskRow(@event.Id.Value, @event.Name, @event.Timestamp));
        await db.SaveChangesAsync().ConfigureAwait(false);
    }
}
```

You own the write path; Cronus only fans the event into your handler.

## Querying a projection

Inject `IProjectionReader` and call `GetAsync<TProjection>(id)`. The returned `ReadResult<TProjection>` tells you whether the projection was found and whether there was an error.

```csharp
public interface IProjectionReader
{
    Task<ReadResult<T>> GetAsync<T>(IBlobId projectionId) where T : IProjectionDefinition;
    Task<ReadResult<T>> GetAsOfAsync<T>(IBlobId projectionId, DateTimeOffset timestamp) where T : IProjectionDefinition;

    Task<ReadResult<IProjectionDefinition>> GetAsync(IBlobId projectionId, Type projectionType);
    Task<ReadResult<IProjectionDefinition>> GetAsOfAsync(IBlobId projectionId, Type projectionType, DateTimeOffset timestamp);
}
```

{% code title="TaskQueryController.cs" %}
```csharp
[ApiController]
[Route("[controller]/[action]")]
public class TaskQueryController : ControllerBase
{
    private readonly IProjectionReader reader;

    public TaskQueryController(IProjectionReader reader)
    {
        this.reader = reader;
    }

    [HttpGet]
    public async Task<IActionResult> GetByUser(string tenant, string userId)
    {
        var id = new UserId(tenant, "user", userId);
        ReadResult<TaskProjection> result = await reader.GetAsync<TaskProjection>(id).ConfigureAwait(false);

        if (result.NotFound) return NotFound();
        if (result.HasError) return Problem(result.Error);

        return Ok(result.Data.State.Tasks);
    }
}
```
{% endcode %}

`GetAsOfAsync` rebuilds the projection as it was at a given point in time — useful for time-travel queries and diagnostics.

{% hint style="info" %}
Expose API DTOs separately from your projection state. If you return `result.Data.State` directly, renaming a property breaks the API contract. Map to a response DTO first.
{% endhint %}

## Projection versioning

When a projection's shape changes (new event subscription, different state schema, new aggregation) Cronus treats it as a new _version_: the old version keeps serving reads while the new one rebuilds in the background, then traffic switches over atomically. That flow is covered in the projections versioning documentation.
