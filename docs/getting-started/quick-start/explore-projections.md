# Explore Projections

[Projections ](../../cronus-framework/domain-modeling/handlers/projections.md)are [queryable ](../../cronus-framework/domain-modeling/handlers/projections.md#querying-a-projection)models used for the reading part of our application. We can design projections in such a way that we can manage what data we want to store and by what will be searched. Events are the basis for projections data.

For using projections we should update the [configuration ](../../cronus-framework/configuration.md#cronus-projectionsenabled)file for both API and Service.

{% code title="appsettings.json" %}
```csharp
  "Persistence": { /* ... */ },
  "Projections": {
      "Cassandra": {
        "ConnectionString": "Contact Points=127.0.0.1;Port=9042;Default Keyspace=taskmanager_projections"
      }
  }
```
{% endcode %}

And add some dependencies.

```csharp
dotnet add package Cronus.Projection.Cassandra
```

### Create a projection for querying tasks

You can choose whitch implementation to use. You can get hte tasks(_commented in the controller_) with same name, or all tasks.

{% tabs %}
{% tab title="TaskProjection" %}
```csharp
[DataContract(Name = "c94513d1-e5ee-4aae-8c0f-6e85b63a4e03")]
public class TaskProjection : ProjectionDefinition<TaskProjectionData, TaskId>,
    IEventHandler<TaskCreated>
{
    public TaskProjection()
    {
        //Id.NID - here we are subscribing by tenant
        //in our case the tenant is: "tenant"
        //so we well get all events
        Subscribe<TaskCreated>(x => new TaskId(x.Id.NID));
    }

    public Task HandleAsync(TaskCreated @event)
    {
        Data task = new Data();

        task.Id = @event.Id;
        task.UserId = @event.UserId;
        task.Name = @event.Name;
        task.Timestamp = @event.Timestamp;

        State.Tasks.Add(task);

        return Task.CompletedTask;
    }
    public IEnumerable<Data> GetTaskByName(string name)
    {
        return State.Tasks.Where(x => x.Name.Equals(name));
    }
}
```
{% endtab %}

{% tab title="TaskProjectionData" %}
```csharp
[DataContract(Name = "c135893e-b9e3-453a-b0e0-53545094ec5d")]
public class TaskProjectionData
{
    public TaskProjectionData()
    {
        Tasks = new List<Data>();
    }

    [DataMember(Order = 1)]
    public List<Data> Tasks { get; set; }

    [DataContract(Name = "317b3cbb-593a-4ffc-8284-d5f5c599d8ae")]
    public class Data
    {
        [DataMember(Order = 1)]
        public TaskId Id { get; set; }

        [DataMember(Order = 2)]
        public UserId UserId { get; set; }

        [DataMember(Order = 3)]
        public string Name { get; set; }

        [DataMember(Order = 4)]
        public DateTimeOffset CreatedAt { get; set; }

        [DataMember(Order = 5)]
        public DateTimeOffset Timestamp { get; set; }
    }
}
```
{% endtab %}
{% endtabs %}

Every time the event will occur it will be handled and persist in its state.

### Read the state

Inject `IProjectionReader` that will be responsible for getting the projection state by Id on which projection was subscribed before: `Subscribe<TaskCreated>(x => x.UserId).`

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class TaskController : ControllerBase
{
private readonly IPublisher<CreateTask> _publisher;
private readonly IProjectionReader _projectionReader;

public TaskController(IPublisher<CreateTask> publisher, IProjectionReader reader)
{
    _publisher = publisher;
    _projectionReader = reader;
}

//.... create task code ..//

[HttpGet]
public async Task<IActionResult> GetTasksByName(string name)
{

    ReadResult<TaskProjection> readResult = await _projectionReader.GetAsync<TaskProjection>(new TaskId("tenant"));

    if (readResult.IsSuccess == false)
        return NotFound();

    var TasksByName = readResult.Data.GetTaskByName(name);


    return Ok(TasksByName);

    ////Get all tasks
    //return Ok(readResult.Data.State.Tasks.Select(x => new TaskData
    //{
    //    CreatedAt = x.CreatedAt,
    //    Id = x.Id,
    //    Name = x.Name,
    //    Timestamp = x.Timestamp,
    //    UserId = x.UserId
    //}));
}
```

### Connect Dashboard

(_The dashboard is not requerd_)

If we hit this controller immediately after the first start, it could lead to a probable read error. \
We need to give it some time to initialize our new projection store and build new versions of the projections. For an empty event store, it could take less than a few seconds but in order not to wait for this and verify that all working properly, we will check it manually.

[Cronus Dashboard](https://cronus-dashboard.github.io/) is a UI management tool for the Cronus framework.\
It hosts inside our Application so add this missing code to our background service.

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    logger.LogInformation("Starting service...");
    cronusHost.Start();
    
    // Dashboard configuration
    cronusDashboard = CronusApi.GetHost();
    cronusApi.Provider = cronusDashboard.Services;
    await cronusDashboard.StartAsync().ConfigureAwait(false);
    
    logger.LogInformation("Service started!");
}
```

Start our Cronus Service and API.&#x20;

In the dashboard select the `Connections` tab and click `New Connection`.\
Set the predefined port for the Cronus endpoint: [http://localhost:7477](http://localhost:7477) and specify your connection name. Click `Check` and then `Add Connection`.\
After you add a connection select it from the drop-down menu and navigate to the Projections tab.\
You would be able to see all projections in the system.&#x20;

![A live green badge means that the projection is synchronized with ES and ready to use.](<../../.gitbook/assets/image (1).png>)

Now we would be able to request a controller with `userId`. `GetAsync` method of `IProjectionReader` will restore all events related to projection and apply them to the state. &#x20;
