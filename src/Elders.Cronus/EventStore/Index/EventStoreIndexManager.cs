using Elders.Cronus.EventStore.Index.Events;
using Elders.Cronus.Projections.Versioning;
using System;

namespace Elders.Cronus.EventStore.Index
{
    public class EventStoreIndexManager : AggregateRoot<EventStoreIndexManagerState>
    {
        EventStoreIndexManager() { }

        public EventStoreIndexManager(EventStoreIndexManagerId id)
        {
            var timebox = new VersionRequestTimebox(DateTime.UtcNow);
            BuildIndex(id, timebox);
        }

        public void Register()
        {
            if (state.IndexExists == false)
            {
                Rebuild();
            }
        }

        public void Rebuild()
        {
            if (state.IsBuilding)
                return;

            BuildIndex(state.Id, new VersionRequestTimebox(DateTime.UtcNow));
        }

        private void BuildIndex(EventStoreIndexManagerId id, VersionRequestTimebox timebox)
        {
            var @event = new EventStoreIndexRequested(id, DateTimeOffset.UtcNow, timebox);
            Apply(@event);
        }
    }
}
