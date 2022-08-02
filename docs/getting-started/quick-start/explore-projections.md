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

### Create a projection for querying tasks created by the user

{% tabs %}
{% tab title="TaskProjection" %}
```csharp
[DataContract(Name = "c94513d1-e5ee-4aae-8c0f-6e85b63a4e03")]
    public class TaskProjection : ProjectionDefinition<TaskProjectionData, UserId>,
        IEventHandler<TaskCreated>
    {
        public TaskProjection()
        {
            Subscribe<TaskCreated>(x => x.UserId);
        }

        public Task HandleAsync(TaskCreated @event)
        {
            TaskDto task = new TaskDto();
            State.Tasks.Add(task);

            return Task.CompletedTask;
        }
    }
```
{% endtab %}

{% tab title="TaskProjectionData" %}
```csharp
 [DataContract(Name = "565e099a-5ca2-4258-87b1-4091a9d2c945")]
    public class TaskProjectionData
    {
        public TaskProjectionData()
        {
            Tasks = new List<TaskDto>();
        }

        [DataMember(Order = 1)]
        public List<TaskDto> Tasks { get; set; }
    }
```
{% endtab %}
{% endtabs %}

Every time the event will occur it will be handled and persist in its state.

### Read the state

Inject `IProjectionReader` that will be responsible for getting the projection state by Id on which projection was subscribed before: `Subscribe<TaskCreated>(x => x.UserId).`

```csharp
[ApiController]
[Route("Tasks")]
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

[HttpGet("user/{userId}/")]
public async Task<IActionResult> GetTasksByUserIdAsync(string userId)
{
    UserId UserId = new UserId(userId);

    ReadResult<TaskProjection> readResult = await _projectionReader.GetAsync<TaskProjection>(UserId);

    if (readResult.IsSuccess == false)
        return NotFound();

    return Ok(readResult.Data.State.Tasks);
}
```

### Connect Dashboard

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
