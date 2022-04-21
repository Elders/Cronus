using System.Threading.Tasks;
using System.Runtime.Serialization;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Snapshotting;

namespace Elders.Cronus.EventStore.Index.Handlers
{
    [DataContract(Name = ContractId)]
    public class EventStoreIndexStatus : ProjectionDefinition<EventStoreIndexStatusState, EventStoreIndexManagerId>, IAmNotSnapshotable, ISystemProjection, ISystemEventStoreIndexHandler,
        IEventHandler<EventStoreIndexRequested>,
        IEventHandler<EventStoreIndexIsNowPresent>
    {
        public const string ContractId = "1bcdb806-dbd0-45e7-b781-e3d2fd0589c1";

        public EventStoreIndexStatus()
        {
            Subscribe<EventStoreIndexRequested>(x => x.Id);
            Subscribe<EventStoreIndexIsNowPresent>(x => x.Id);
        }

        public Task HandleAsync(EventStoreIndexRequested @event)
        {
            State.Id = @event.Id;
            State.Status = IndexStatus.Building;

            return Task.CompletedTask;
        }

        public Task HandleAsync(EventStoreIndexIsNowPresent @event)
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
}
