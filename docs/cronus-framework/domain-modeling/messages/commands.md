# Commands

A **command** expresses the intent to change the state of a single aggregate. It is an immutable message, authored by the caller, that the domain model can accept or reject based on the current state and its invariants. One command maps to one aggregate and one application service.

{% hint style="success" %}
**You can / should / must**

* a command **must** be immutable
* a command **must** name a business intent in the imperative form (`CreateTask`, `SuspendAccount`)
* a command **must** update at most one aggregate
* a command **can** be rejected when validation or an invariant fails — return early in the application service
* a command **should not** be broadcast from the UI directly; go through an API
{% endhint %}

## Defining a command

Implement the `ICommand` marker. Keep a private parameterless constructor (serializers need it) and assign all properties through the public constructor so instances are effectively immutable.

{% code title="CreateTask.cs" %}
```csharp
[DataContract(Name = "857d960c-4b91-49cc-98fd-fa543906c52d")]
public class CreateTask : ICommand
{
    CreateTask() { }

    public CreateTask(TaskId id, UserId userId, string name, DateTimeOffset timestamp)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (userId is null) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));

        Id = id;
        UserId = userId;
        Name = name;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)] public TaskId Id { get; private set; }
    [DataMember(Order = 2)] public UserId UserId { get; private set; }
    [DataMember(Order = 3)] public string Name { get; private set; }
    [DataMember(Order = 4)] public DateTimeOffset Timestamp { get; private set; }

    public override string ToString() => $"Create task '{Name}' ({Id}) for user {UserId}.";
}
```
{% endcode %}

{% hint style="info" %}
Cronus uses `ToString()` when writing structured logs. Override it to get readable output; otherwise the class name alone will appear in the logs.
{% endhint %}

## Publishing a command

Inject `IPublisher<ICommand>` and `await` the `PublishAsync` method. The call returns `true` when the transport accepted the command. A `false` result means the command was not dispatched and the caller must decide how to recover.

{% code title="TaskController.cs" %}
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
        var taskId = new TaskId(request.Tenant, "task", Guid.NewGuid().ToString());
        var userId = new UserId(request.Tenant, "user", request.UserId);

        var command = new CreateTask(taskId, userId, request.Name, DateTimeOffset.UtcNow);

        if (await publisher.PublishAsync(command).ConfigureAwait(false) == false)
            return Problem($"Unable to publish {command}.");

        return Accepted(taskId.Value);
    }
}
```
{% endcode %}

{% hint style="info" %}
Commands are handled asynchronously by an application service running inside the Cronus host. The API returns `202 Accepted` because the command has been queued — not yet executed.
{% endhint %}

## Delaying or scheduling a command

`IPublisher<TMessage>` exposes overloads for deferred delivery:

```csharp
await publisher.PublishAsync(command, TimeSpan.FromMinutes(5));            // delay
await publisher.PublishAsync(command, DateTime.UtcNow.AddHours(1));        // schedule
```

Deferred delivery requires the scheduled-message transport to be configured. See the transport-specific documentation (e.g. RabbitMQ) for setup.

## Handling a command

Commands are consumed by an _application service_ — the write-side entry point for a specific aggregate.

{% content-ref url="../handlers/application-services.md" %}
[application-services.md](../handlers/application-services.md)
{% endcontent-ref %}
