# Explore Projections

With a `TaskCreated` event in Cassandra we can now build a **projection** — a queryable read model derived from events. This page walks through adding a `TaskProjection` and exposing it through the API.

{% content-ref url="../../cronus-framework/domain-modeling/handlers/projections.md" %}
[projections.md](../../cronus-framework/domain-modeling/handlers/projections.md)
{% endcontent-ref %}

## 1. Install the projections package

Projections are persisted by the `Cronus.Projections.Cassandra` package. It should already be added to `TaskManager.Service` from the [setup](setup.md) step; double-check:

```shell
dotnet list TaskManager.Service package | grep Projections
```

{% hint style="warning" %}
The correct package name is **`Cronus.Projections.Cassandra`** (plural). An older, obsolete variant called `Cronus.Projection.Cassandra` still exists on NuGet — don't install it.
{% endhint %}

Make sure the `Cronus:Projections:Cassandra:ConnectionString` is set in both API and worker `appsettings.json`:

```json
"Projections": {
  "Cassandra": {
    "ConnectionString": "Contact Points=127.0.0.1;Port=9042;Default Keyspace=taskmanager_projections"
  }
}
```

## 2. Define the projection

We want to query all tasks belonging to a given user. The projection ID will therefore be `UserId`, and the projection subscribes to `TaskCreated`.

{% tabs %}
{% tab title="TaskProjection" %}
```csharp
[DataContract(Name = "c94513d1-e5ee-4aae-8c0f-6e85b63a4e03")]
public class TaskProjection : ProjectionDefinition<TaskProjectionState, UserId>,
    IEventHandler<TaskCreated>
{
    public TaskProjection()
    {
        // one event can fan out to many projection instances — here, one per user
        Subscribe<TaskCreated>(e => e.UserId);
    }

    public Task HandleAsync(TaskCreated @event)
    {
        // HandleAsync runs on every event; design it to be idempotent
        if (State.Tasks.Any(x => x.Id.Equals(@event.Id)))
            return Task.CompletedTask;

        State.Tasks.Add(new TaskProjectionState.Entry
        {
            Id = @event.Id,
            Name = @event.Name,
            CreatedAt = @event.Timestamp,
            Deadline = @event.Deadline
        });

        return Task.CompletedTask;
    }

    public IEnumerable<TaskProjectionState.Entry> WithName(string name)
        => State.Tasks.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
```
{% endtab %}

{% tab title="TaskProjectionState" %}
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
        [DataMember(Order = 4)] public DateTimeOffset Deadline { get; set; }
    }
}
```
{% endtab %}
{% endtabs %}

{% hint style="info" %}
`Subscribe<TEvent>(e => projectionId)` is how Cronus maps an event to a projection instance. Every time `TaskCreated` is handled, the framework asks the projection for the target ID (here, `e.UserId`), loads (or creates) the projection row for that ID, applies the event, and saves it.
{% endhint %}

## 3. Query the projection

Inject `IProjectionReader` into a controller and call `GetAsync<TProjection>(id)`. The reader returns a `ReadResult<TaskProjection>`; always branch on its `NotFound` / `HasError` / `IsSuccess` flags.

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
    public async Task<IActionResult> GetByUser(string userId, CancellationToken ct)
    {
        const string tenant = "tenant";
        var id = new UserId(tenant, userId);

        ReadResult<TaskProjection> result = await reader.GetAsync<TaskProjection>(id).ConfigureAwait(false);

        if (result.NotFound) return NotFound();
        if (result.HasError) return Problem(result.Error);

        return Ok(result.Data.State.Tasks);
    }
}
```
{% endcode %}

{% hint style="info" %}
The first time the worker starts with a new projection, Cronus builds and activates a new _projection version_ by replaying the existing event stream into it. This can take a moment on large stores. The projection is considered live only once the version is `Live`; until then `NotFound` is a possible result.
{% endhint %}

## 4. Run it end-to-end

1. Start the worker and the API (`dotnet run --project ...`).
2. `POST /Task/CreateTask` with `userId=alice`.
3. Wait a moment for the worker to handle the command and update the projection.
4. `GET /TaskQuery/GetByUser?userId=alice` → the created task appears in the list.

If the projection returns `NotFound`, check the worker logs for `CronusWorkflowHandle` entries involving `TaskProjection`. A missing entry means the event was not routed — usually a `Subscribe<TEvent>` is missing or the tenant does not match.

## 5. Optional — plug in the Cronus Dashboard

[Cronus Dashboard](https://cronus-dashboard.github.io/) is a browser UI that inspects running hosts: tenants, projections, versions, rebuilds, and event traffic. It talks to the Cronus RPC endpoint that the worker exposes.

Enable the RPC endpoint in the worker's `appsettings.json`:

```json
{
  "Cronus": {
    "RpcApiEnabled": true
  }
}
```

Then open the dashboard, add a connection to `http://localhost:7477`, and navigate to the _Projections_ tab. A green "live" badge means the projection is synchronised with the event store.

## Where to next

* [Aggregate](../../cronus-framework/domain-modeling/aggregate.md) — deeper on the write model.
* [Projections handler](../../cronus-framework/domain-modeling/handlers/projections.md) — snapshots, versioning, non-event-sourced projections.
* [Configuration](../../cronus-framework/configuration.md) — every Cronus option.
