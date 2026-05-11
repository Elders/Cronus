# Application Services

An **application service** is the write-side entry point of an aggregate. It is the handler where commands are received and translated into operations on a single aggregate root. This is the "write" side in [CQRS](../../concepts/cqrs.md).

An application service orchestrates a command: it loads (or creates) the aggregate, calls the right method on it, and saves the resulting events. It is the only place that bridges the infrastructure (the command bus, the repository) and the domain model. Nothing outside of an application service should ever touch the aggregate directly.

{% content-ref url="../aggregate.md" %}
[aggregate.md](../aggregate.md)
{% endcontent-ref %}

## Defining an application service

Inherit from `ApplicationService<AR>` where `AR` is the aggregate root type. Implement `ICommandHandler<T>` for every command you want to handle. The base class gives you a protected `repository` field of type `IAggregateRepository` and a `UpdateAsync` helper that loads, mutates, and saves in one call.

```csharp
public abstract class ApplicationService<AR> : IApplicationService where AR : IAggregateRoot
{
    protected readonly IAggregateRepository repository;

    public ApplicationService(IAggregateRepository repository) { ... }

    public virtual async Task UpdateAsync(AggregateRootId id, Action<AR> update) { ... }
}
```

`ICommandHandler<T>` requires one async method:

```csharp
public interface ICommandHandler<in T> where T : ICommand
{
    Task HandleAsync(T command);
}
```

## A canonical application service

{% code title="TaskAppService.cs" %}
```csharp
public class TaskAppService : ApplicationService<TaskAggregate>,
    ICommandHandler<CreateTask>,
    ICommandHandler<RenameTask>,
    ICommandHandler<CloseTask>
{
    public TaskAppService(IAggregateRepository repository) : base(repository) { }

    public async Task HandleAsync(CreateTask command)
    {
        ReadResult<TaskAggregate> result = await repository.LoadAsync<TaskAggregate>(command.Id).ConfigureAwait(false);
        if (result.IsSuccess)
            return; // already created — commands are idempotent

        var task = new TaskAggregate(command.Id, command.UserId, command.Name, command.Deadline);
        await repository.SaveAsync(task).ConfigureAwait(false);
    }

    public Task HandleAsync(RenameTask command)
    {
        return UpdateAsync(command.Id, task => task.Rename(command.NewName));
    }

    public Task HandleAsync(CloseTask command)
    {
        return UpdateAsync(command.Id, task => task.Close(command.ClosedBy));
    }
}
```
{% endcode %}

### When to load, when to create

Use the explicit `LoadAsync` + `SaveAsync` pattern when the command can create the aggregate. `ReadResult<AR>` exposes `IsSuccess`, `NotFound` and `HasError`:

```csharp
ReadResult<TaskAggregate> result = await repository.LoadAsync<TaskAggregate>(command.Id);
if (result.NotFound)
{
    var task = new TaskAggregate(...);
    await repository.SaveAsync(task);
}
```

Use `UpdateAsync(id, action)` when you are 100% sure the aggregate already exists. It throws if the load fails and saves automatically on success:

```csharp
await UpdateAsync(command.Id, task => task.Rename(command.NewName));
```

{% hint style="info" %}
The `update` delegate passed to `UpdateAsync` is **synchronous** (`Action<AR>`). The aggregate's methods are expected to be synchronous — they compute and `Apply` events without any I/O. If you need async work, do it _before_ calling `UpdateAsync`, not inside the delegate.
{% endhint %}

## Best Practices

{% hint style="success" %}
**You can / should / must**

* an application service **can** load an aggregate root from the event store
* an application service **can** save new aggregate root events to the event store
* an application service **can** read from a projection to resolve a missing piece of context (not common — think twice)
* an application service **can** call an external service _before_ mutating the aggregate (e.g. to resolve an ID)
* an application service **must** be stateless — no fields beyond the injected repository
* an application service **must** update only one aggregate per command
{% endhint %}

{% hint style="warning" %}
**You should not**

* an application service **should not** mutate more than one aggregate in a single `HandleAsync` call — use a [saga](sagas.md) instead
* an application service **should not** contain domain logic — keep decisions inside the aggregate
* an application service **should not** send emails, push notifications, or fire HTTP calls — use a [port](ports.md) or a [gateway](gateways.md) reacting to the resulting events
* an application service **should not** update a projection — projections are read models that rebuild themselves from events
{% endhint %}
