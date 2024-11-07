# Commands

A command is a simple immutable object that is sent to the domain to trigger a state change. There should be a single command handler for each command. It is recommended to use imperative verbs when naming commands together with the name of the aggregate they operate on.

{% content-ref url="../handlers/application-services.md" %}
[application-services.md](../handlers/application-services.md)
{% endcontent-ref %}

It is possible for a command to get rejected if the data it holds is incorrect or inconsistent with the current state of the aggregate.

{% hint style="success" %}
**You can/should/must...**

* a command **must** be immutable
* a command **should** clearly state a business intent with a name in the imperative form
* a command **can** be rejected due to domain validation, error or other reason
* a command **must** update only one aggregate
{% endhint %}

## Defining a command

You can define a command with Cronus using the `ICommand` markup interface. All commands get serialized and deserialized, that's why you need to keep the parameterless constructor and specify data contracts.

{% content-ref url="../../messaging/serialization.md" %}
[serialization.md](../../messaging/serialization.md)
{% endcontent-ref %}

```csharp
[DataContract(Name = "857d960c-4b91-49cc-98fd-fa543906c52d")]
public class CreateTask : ICommand
{
    public CreateTask() { }

    public CreateTask(TaskId id, UserId userId, string name, DateTimeOffset timestamp)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (userId is null) throw new ArgumentNullException(nameof(userId));
        if (name is null) throw new ArgumentNullException(nameof(name));
        if (timestamp == default) throw new ArgumentNullException(nameof(timestamp));

        Id = id;
        UserId = userId;
        Name = name;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public TaskId Id { get; private set; }

    [DataMember(Order = 2)]
    public UserId UserId { get; private set; }

    [DataMember(Order = 3)]
    public string Name { get; private set; }

    [DataMember(Order = 4)]
    public DateTimeOffset Timestamp { get; private set; }

    public override string ToString()
    {
        return $"Create a task with id '{Id}' and name '{Name}' for user [{UserId}].";
    }
}
```

{% hint style="info" %}
Cronus uses the `ToString()` method for logging, so you can override it to generate user-readable logs. Otherwise, the name of the command class will be used for log messages.&#x20;
{% endhint %}

## Publishing a command

To publish a command, inject an instance of`IPublisher<ICommand>` into your code and invoke the `Publish()` method passing the command. This method will return `true` if the command has been published successfully through the configured transport. You can also use one of the overrides of the `Publish()` method to delay or schedule a command.

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class TaskController : ControllerBase
{
    private readonly IPublisher<ICommand> _publisher;

    public TaskController(IPublisher<ICommand> publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public IActionResult CreateTask(CreateTaskRequest request)
    {
        string id = Guid.NewGuid().ToString();
        string Userid = Guid.NewGuid().ToString();
        TaskId taskId = new TaskId(id);
        UserId userId = new UserId(Userid);
        var expireDate = DateTimeOffset.UtcNow;
        expireDate.AddDays(request.DaysActive);

        CreateTask command = new CreateTask(taskId, userId, request.Name, expireDate);

        if (_publisher.Publish(command) == false)
        {
            return Problem($"Unable to publish command. {command.Id}: {command.Name}");
        };
        return Ok(id);
    }
}
```
