using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Snapshotting;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index.Handlers
{
    [DataContract(Name = ContractId)]
    public class EventStoreIndexStatus : ProjectionDefinition<EventStoreIndexStatusState, EventStoreIndexManagerId>, IAmNotSnapshotable, ISystemEventStoreIndex,
        IEventHandler<EventStoreIndexRequested>,
        IEventHandler<EventStoreIndexIsNowPresent>
    //IEventHandler<ProjectionVersionRequestCanceled>,
    //IEventHandler<ProjectionVersionRequestTimedout>
    {
        public const string ContractId = "1bcdb806-dbd0-45e7-b781-e3d2fd0589c1";

        public EventStoreIndexStatus()
        {
            Subscribe<EventStoreIndexRequested>(x => x.Id);
        }

        public void Handle(EventStoreIndexRequested @event)
        {
            State.Id = @event.Id;
            State.Status = IndexStatus.Building;
        }

        public void Handle(EventStoreIndexIsNowPresent @event)
        {
            State.Id = @event.Id;
            State.Status = IndexStatus.Present;
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
