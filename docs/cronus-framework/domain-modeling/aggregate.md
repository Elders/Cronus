# Aggregate

An **aggregate** is a cluster of domain objects treated as a single consistency boundary. Every change enters through the root; every invariant is enforced inside the root. With Cronus, an aggregate is event-sourced — its state is the fold of the events it has produced.

Three classes work together:

* `AggregateRoot<TState>` — the entry point for behaviour. It calls `Apply(IEvent)` to record a change.
* `AggregateRootState<TRoot, TRootId>` — the snapshot of current data. It reacts to events via `public void When(TEvent e)`.
* `AggregateRootId` — the aggregate's URN-based identity.

## Aggregate root

Inherit `AggregateRoot<TState>`. Keep a private parameterless constructor so Cronus can rehydrate the aggregate during replay, and expose behaviour as plain methods that call `Apply` to record events.

{% code title="TaskAggregate.cs" %}
```csharp
public class TaskAggregate : AggregateRoot<TaskState>
{
    TaskAggregate() { }

    public TaskAggregate(TaskId id, UserId userId, string name, DateTimeOffset deadline)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (userId is null) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));

        Apply(new TaskCreated(id, userId, name, deadline, DateTimeOffset.UtcNow));
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("Name is required", nameof(newName));
        if (state.Name == newName) return; // idempotent

        Apply(new TaskRenamed(state.Id, state.Name, newName, DateTimeOffset.UtcNow));
    }

    public void Close(UserId closedBy)
    {
        if (state.IsClosed) return; // already closed
        Apply(new TaskClosed(state.Id, closedBy, DateTimeOffset.UtcNow));
    }
}
```
{% endcode %}

{% hint style="success" %}
**You can / should / must**

* an aggregate root **must** enforce its invariants before calling `Apply`
* an aggregate root **must** remain synchronous — no I/O, no `async`
* an aggregate root **should** be idempotent — calling the same method twice with the same input produces the same events or none at all
* an aggregate root **must not** reference other aggregates directly; use ports or sagas for cross-aggregate flows
{% endhint %}

## Aggregate root state

Inherit `AggregateRootState<TRoot, TRootId>`. State exposes the current data, maintains the `Id`, and folds events into itself via `public void When(TEvent)` handlers. Cronus discovers the handlers by reflection once per process lifetime.

{% code title="TaskState.cs" %}
```csharp
public class TaskState : AggregateRootState<TaskAggregate, TaskId>
{
    public override TaskId Id { get; set; }
    public UserId UserId { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset Deadline { get; set; }
    public bool IsClosed { get; set; }

    public void When(TaskCreated e)
    {
        Id = e.Id;
        UserId = e.UserId;
        Name = e.Name;
        CreatedAt = e.Timestamp;
        Deadline = e.Deadline;
    }

    public void When(TaskRenamed e) => Name = e.NewName;

    public void When(TaskClosed e) => IsClosed = true;
}
```
{% endcode %}

{% hint style="info" %}
The state class is an implementation of the **state pattern** — behaviour (validation, event emission) stays in the aggregate root; pure data and event folding stays in the state. Background reading: [Refactoring Guru](https://refactoring.guru/design-patterns/state/csharp/example), [DoFactory](https://www.dofactory.com/net/state-design-pattern).
{% endhint %}

## Aggregate root id

An `AggregateRootId` is a URN with three segments: `tenant`, `aggregateRootName`, and `id`. The base class's primary constructor is:

```csharp
public AggregateRootId(string tenant, string arName, string id)
```

Define your own typed ID to avoid stringly-typed code elsewhere:

{% code title="TaskId.cs" %}
```csharp
[DataContract(Name = "d5e50e1f-5886-4608-9361-9fe0eb440a6b")]
public class TaskId : AggregateRootId
{
    TaskId() { }

    public TaskId(string tenant, string arName, string id) : base(tenant, arName, id) { }

    public TaskId(string tenant, string id) : base(tenant, "task", id) { }
}
```
{% endcode %}

{% hint style="warning" %}
The constructor order is `(tenant, arName, id)`. Older docs referenced a generic `AggregateRootId<T>` base with a different order — that generic form is commented out in the current source and is **not** available. Use the non-generic `AggregateRootId` with `Parse` / `TryParse` for URN hydration.
{% endhint %}

### Parsing a URN back into an ID

```csharp
if (AggregateRootId.TryParse(urnString, out AggregateRootId parsed))
{
    // parsed.Tenant, parsed.AggregateRootName, parsed.Id
}
```

## Loading and saving

An aggregate is persisted through `IAggregateRepository`:

```csharp
public interface IAggregateRepository
{
    Task SaveAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot;
    Task<ReadResult<AR>> LoadAsync<AR>(AggregateRootId id) where AR : IAggregateRoot;
}
```

Application services are the only place that should call the repository — see:

{% content-ref url="handlers/application-services.md" %}
[application-services.md](handlers/application-services.md)
{% endcontent-ref %}
