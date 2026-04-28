# Persist First Event

With the skeleton from the [setup page](setup.md) running, we can model a tiny slice of the task manager and send a command that ends up as an event in Cassandra.

The slice is:

1. Two aggregate IDs — `TaskId` and `UserId`.
2. A command — `CreateTask`.
3. An event — `TaskCreated`.
4. An aggregate root and its state — `TaskAggregate` / `TaskState`.
5. An application service — `TaskAppService`.
6. An API controller that publishes the command.

Put commands/events/IDs in a shared project (for example `TaskManager.Contracts`) that both the API and the worker reference. Aggregates, states and app services live in the worker project only.

## 1. IDs

`AggregateRootId`'s ctor takes `(tenant, arName, id)` — in that order.

{% tabs %}
{% tab title="TaskId" %}
```csharp
[DataContract(Name = "d5e50e1f-5886-4608-9361-9fe0eb440a6b")]
public class TaskId : AggregateRootId
{
    TaskId() { }

    public TaskId(string tenant, string id) : base(tenant, "task", id) { }
}
```
{% endtab %}

{% tab title="UserId" %}
```csharp
[DataContract(Name = "00f5463f-633a-49f4-9fbe-f98e0911c2f5")]
public class UserId : AggregateRootId
{
    UserId() { }

    public UserId(string tenant, string id) : base(tenant, "user", id) { }
}
```
{% endtab %}
{% endtabs %}

{% hint style="warning" %}
The constructor order is `(tenant, arName, id)`. Older docs and NuGet packages exposed a generic `AggregateRootId<T>` with a different order (`id, arName, tenant`); the generic form is commented out in current master and you should use the non-generic base instead.
{% endhint %}

## 2. Command and event

{% tabs %}
{% tab title="CreateTask" %}
```csharp
[DataContract(Name = "857d960c-4b91-49cc-98fd-fa543906c52d")]
public class CreateTask : ICommand
{
    CreateTask() { }

    public CreateTask(TaskId id, UserId userId, string name, DateTimeOffset deadline, DateTimeOffset timestamp)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (userId is null) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));

        Id = id;
        UserId = userId;
        Name = name;
        Deadline = deadline;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)] public TaskId Id { get; private set; }
    [DataMember(Order = 2)] public UserId UserId { get; private set; }
    [DataMember(Order = 3)] public string Name { get; private set; }
    [DataMember(Order = 4)] public DateTimeOffset Deadline { get; private set; }
    [DataMember(Order = 5)] public DateTimeOffset Timestamp { get; private set; }

    public override string ToString() => $"Create task '{Name}' ({Id}) for user {UserId}.";
}
```
{% endtab %}

{% tab title="TaskCreated" %}
```csharp
[DataContract(Name = "728fc4e7-628b-4962-bd68-97c98aa05694")]
public class TaskCreated : IEvent
{
    TaskCreated() { }

    public TaskCreated(TaskId id, UserId userId, string name, DateTimeOffset deadline, DateTimeOffset timestamp)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Deadline = deadline;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)] public TaskId Id { get; private set; }
    [DataMember(Order = 2)] public UserId UserId { get; private set; }
    [DataMember(Order = 3)] public string Name { get; private set; }
    [DataMember(Order = 4)] public DateTimeOffset Deadline { get; private set; }
    [DataMember(Order = 5)] public DateTimeOffset Timestamp { get; private set; }

    public override string ToString() => $"Task '{Name}' ({Id}) created for user {UserId}.";
}
```
{% endtab %}
{% endtabs %}

## 3. Aggregate and state

The aggregate is the only object allowed to call `Apply`. The state folds each event into itself via a `When(TEvent)` handler.

{% code title="TaskAggregate.cs" %}
```csharp
public class TaskAggregate : AggregateRoot<TaskState>
{
    TaskAggregate() { }

    public void CreateTask(TaskId id, UserId userId, string name, DateTimeOffset deadline)
    {
        Apply(new TaskCreated(id, userId, name, deadline, DateTimeOffset.UtcNow));
    }
}
```
{% endcode %}

{% code title="TaskState.cs" %}
```csharp
public class TaskState : AggregateRootState<TaskAggregate, TaskId>
{
    public override TaskId Id { get; set; }
    public UserId UserId { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset Deadline { get; set; }

    public void When(TaskCreated e)
    {
        Id = e.Id;
        UserId = e.UserId;
        Name = e.Name;
        CreatedAt = e.Timestamp;
        Deadline = e.Deadline;
    }
}
```
{% endcode %}

## 4. Application service

`ApplicationService<TaskAggregate>` gives you the `repository` field. `ICommandHandler<CreateTask>.HandleAsync` is the async entry point — load the aggregate, decide whether to create it, save it.

{% code title="TaskAppService.cs" %}
```csharp
public class TaskAppService : ApplicationService<TaskAggregate>,
    ICommandHandler<CreateTask>
{
    public TaskService(IAggregateRepository repository) : base(repository) { }

    public async Task HandleAsync(CreateTask command)
    {
        ReadResult<TaskAggregate> existing = await repository.LoadAsync<TaskAggregate>(command.Id).ConfigureAwait(false);
        if (existing.IsSuccess) return; // idempotent — already created

        var task = new TaskAggregate(command.Id, command.UserId, command.Name, command.Deadline);
        await repository.SaveAsync(task).ConfigureAwait(false);
    }
}
```
{% endcode %}

{% hint style="info" %}
`ReadResult<T>` exposes `IsSuccess`, `NotFound`, `HasError`, and `Error` — use them to branch deliberately instead of catching exceptions.
{% endhint %}

## 5. API controller

Inject `IPublisher<ICommand>` and `await publisher.PublishAsync(...)`. The method returns `true` when the transport accepted the command.

{% tabs %}
{% tab title="Controller" %}
```csharp
[ApiController]
[Route("[controller]/[action]")]
public class TaskController : ControllerBase
{
    private readonly IPublisher<ICommand> publisher;

    public TaskController(IPublisher<ICommand> publisher)
    {
        this.publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskRequest request, CancellationToken ct)
    {
        const string tenant = "tenant"; // must match Cronus:Tenants from appsettings

        var taskId = new TaskId(tenant, Guid.NewGuid().ToString());
        var userId = new UserId(tenant, request.UserId);
        var deadline = DateTimeOffset.UtcNow.AddDays(request.DaysActive);

        var command = new CreateTask(taskId, userId, request.Name, deadline, DateTimeOffset.UtcNow);

        if (await publisher.PublishAsync(command).ConfigureAwait(false) == false)
            return Problem($"Unable to publish {command}.");

        return Accepted(taskId.Value);
    }
}
```
{% endtab %}

{% tab title="Request" %}
```csharp
public class CreateTaskRequest
{
    [Required] public string UserId { get; set; }
    [Required] public string Name { get; set; }
    [Required] public int DaysActive { get; set; }
}
```
{% endtab %}
{% endtabs %}

## 6. Run it

With both the worker and the API running (see [setup.md](setup.md)), `POST /Task/CreateTask`:

```json
{
  "userId": "alice",
  "name": "Write the quick start",
  "daysActive": 7
}
```

The API returns `202 Accepted` with the task URN (for example `urn:tenant:task:c9b3…`). Internally:

1. The controller publishes `CreateTask` onto RabbitMQ.
2. The worker's subscriber picks it up and runs `TaskAppService.HandleAsync`.
3. The service constructs a new `TaskAggregate`, which applies a `TaskCreated` event.
4. `repository.SaveAsync(task)` writes the event to Cassandra.

{% hint style="success" %}
If you watch the worker logs you should see a `CronusWorkflowHandle` line saying `TaskAppService handled CreateTask in X.XXXXms.` — that is the diagnostics workflow confirming the handler ran.
{% endhint %}

## 7. Inspect the Event Store

Take the task URN from the response, Base64-encode it, and query the Cassandra `taskmanagerevents` table:

```shell
echo -n 'urn:tenant:task:c9b3...' | base64
# e.g. dXJuOnRlbmFudDp0YXNrOmM5YjMu...

cqlsh -e "select * from taskmanager_es.taskmanagerevents where id = 'dXJuOnRlbmFudDp0YXNrOmM5YjMu...';"
```

You should see exactly one row — the `TaskCreated` event, serialised, tagged with its `[DataContract(Name = …)]` GUID. Restart the worker and run a `LoadAsync<TaskAggregate>(taskId)`: Cronus rebuilds the aggregate by replaying that single event back into the state.

Next stop — turning those events into a read model.

{% content-ref url="explore-projections.md" %}
[explore-projections.md](explore-projections.md)
{% endcontent-ref %}
