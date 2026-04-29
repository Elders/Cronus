using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Elders.Cronus.Projections;

namespace Elders.Cronus.EventStore.Index.Handlers;

[DataContract(Name = ContractId)]
public sealed class EventStoreIndexStatus : ProjectionDefinition<EventStoreIndexStatusState, EventStoreIndexManagerId>, ISystemProjection, ISystemEventStoreIndexHandler,
    IEventHandler<EventStoreIndexRequested>,
    IEventHandler<EventStoreIndexIsNowPresent>
{
    public const string ContractId = "1bcdb806-dbd0-45e7-b781-e3d2fd0589c1";

    public EventStoreIndexStatus()
    {
        Subscribe<EventStoreIndexRequested>(x => x.Id);
        Subscribe<EventStoreIndexIsNowPresent>(x => x.Id);
    }

    /// <summary>
    /// Records that an index rebuild has been requested for the supplied id.
    /// </summary>
    /// <param name="event">The event signalling that an index rebuild was requested.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(EventStoreIndexRequested @event, CancellationToken cancellationToken = default)
    {
        State.Id = @event.Id;
        State.Status = IndexStatus.Building;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Records that the index has finished rebuilding and is present.
    /// </summary>
    /// <param name="event">The event signalling that the index is now present.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(EventStoreIndexIsNowPresent @event, CancellationToken cancellationToken = default)
    {
        State.Id = @event.Id;
        State.Status = IndexStatus.Present;

        return Task.CompletedTask;
    }
}

public class EventStoreIndexStatusState
{
    public EventStoreIndexStatusState()
    {
        Status = IndexStatus.NotPresent;
    }

    public EventStoreIndexManagerId Id { get; set; }

    public IndexStatus Status { get; set; }
}
