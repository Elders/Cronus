# Persist First Event

### Create Ids, commands and events

First, we need to add a UserId and TaskId to have the [Identifications ](../../cronus-framework/domain-modeling/ids.md)of these two entities

{% tabs %}
{% tab title="TaskId" %}
```csharp
[DataContract(Name = "d5e50e1f-5886-4608-9361-9fe0eb440a6b")]
public class TaskId : AggregateRootId
{
    TaskId() { }

    public TaskId(string id) : base("tenant", "task", id) { }
}
```
{% endtab %}

{% tab title="UserId" %}
```csharp
[DataContract(Name = "00f5463f-633a-49f4-9fbe-f98e0911c2f5")]
public class UserId : AggregateRootId
{
    UserId() { }

    public UserId(string id) : base("tenant", "user", id) { }
}
```
{% endtab %}
{% endtabs %}

Then we need to create a Cronus [command](../../cronus-framework/domain-modeling/messages/commands.md) for task creation and an [Event](../../cronus-framework/domain-modeling/messages/events.md) that will indicate that the event has occurred.

{% tabs %}
{% tab title="Command" %}
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
{% endtab %}

{% tab title="Event" %}
```csharp
[DataContract(Name = "728fc4e7-628b-4962-bd68-97c98aa05694")]
public class TaskCreated : IEvent
{
    TaskCreated() { }

    public TaskCreated(TaskId id, UserId userId, string name, DateTimeOffset timestamp)
    {
        Id = id;
        UserId = userId;
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public TaskId Id { get; private set; }

    [DataMember(Order = 2)]
    public UserId UserId { get; private set; }

    [DataMember(Order = 3)]
    public string Name { get; private set; }

    [DataMember(Order = 4)]
    public DateTimeOffset CreatedAt { get; private set; }

    [DataMember(Order = 5)]
    public DateTimeOffset Timestamp { get; private set; }

    public override string ToString()
    {
        return $"Task with id '{Id}' and name '{Name}' for user [{UserId}] at {CreatedAt} has been created.";
    }
}
```
{% endtab %}
{% endtabs %}

### Create an Aggregate and Application Service

Add [Aggregate ](../../cronus-framework/domain-modeling/aggregate.md)that inherits [AggregateRoot ](../../cronus-framework/domain-modeling/aggregate.md#aggregate-root)with the generic [state](../../cronus-framework/domain-modeling/aggregate.md#aggregate-root-state).

```csharp
public class TaskAggregate : AggregateRoot<TaskState>
{
    public TaskAggregate() { }

    public void CreateTask(TaskId id, UserId userId, string name, DateTimeOffset deadline)
    {
        IEvent @event = new TaskCreated(id, userId, name, deadline);
        Apply(@event);
    }
}
```

Apply method will pass the event to the state of an aggregate and change its state.

```csharp
public class TaskState : AggregateRootState<TaskAggregate, TaskId>
{
    public override TaskId Id { get; set; }

    public UserId UserId { get; set; }

    public string Name { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset Deadline { get; set; }

    public void When(TaskCreated @event)
    {
        Id = @event.Id;
        UserId = @event.UserId;
        Name = @event.Name;
        CreatedAt = @event.CreatedAt;
        Deadline = @event.Timestamp;
    }
}
```

Finally, we can create an [Application Service](../../cronus-framework/domain-modeling/handlers/application-services.md) for command handling.

```csharp
[DataContract(Name = "ef669879-5d35-4cb7-baea-39a7c46c9e13")]
public class TaskService : ApplicationService<TaskAggregate>,
ICommandHandler<CreateTask>
{
    public TaskService(IAggregateRepository repository) : base(repository) { }

    public async Task HandleAsync(CreateTask command)
    {
        ReadResult<TaskAggregate> taskResult = await repository.LoadAsync<TaskAggregate>(command.Id).ConfigureAwait(false);
        if (taskResult.NotFound)
        {
            var task = new TaskAggregate();
            task.CreateTask(command.Id, command.UserId, command.Name, DateTimeOffset.UtcNow);
            await repository.SaveAsync(task).ConfigureAwait(false);
        }
    }
}
```

We register a handler by inheriting from `ICommandHandler<>.` When the command arrives we read the state of the aggregate, and if it is not found we create a new one and call `SaveAsync` to save its state to the database.&#x20;

### Create Controller and send a request

Now we need a controller to publish our commands and create tasks.&#x20;

{% tabs %}
{% tab title="Controller" %}
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
{% endtab %}

{% tab title="Request" %}
```csharp
public class CreateTaskRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public int DaysActive { get; set; }
}
```
{% endtab %}
{% endtabs %}

Here we create _TaskId_ and _UserId_ and inject_`IPublisher<CreateTask>`_to publish the command. After this, the command will be sent to RabbitMq and then handled in Application Service.

Now let's start our Service and API. \
We should be able to make post requests to our Controller throw the Swagger and create our first Task in the system. It must be persisted in the [Event Store](../../cronus-framework/event-store/).

![I highly recommend debugging on the first run to better understand the flow of program execution.](<../../.gitbook/assets/image (10).png>)

### Inspection of the Event Store

Download [DevCenter ](https://downloads.datastax.com/#devcenter)or any other UI tool for Cassandra.

Let's take an Id from the response and encode it to Base64.\
Than try: `select * from taskmanagerevents where id = 'dXJuOnRlbmFudDp0YXNrOmU1MjA1NTA3LWYyNmUtNGExMy05OTU4LTNjMzVlYzAwY2I1Yw=='`

![Use DevCenter tool for Cassandra visualization.](<../../.gitbook/assets/image (9).png>)
