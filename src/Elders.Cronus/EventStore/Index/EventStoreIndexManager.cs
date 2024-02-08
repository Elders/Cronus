using System;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.EventStore.Index
{
    public class EventStoreIndexManager : AggregateRoot<EventStoreIndexManagerState>
    {
        EventStoreIndexManager() { }

        public EventStoreIndexManager(EventStoreIndexManagerId id)
        {
            var timebox = new VersionRequestTimebox(DateTime.UtcNow);
            BuildIndex(id, timebox, 2);
        }

        public void Register()
        {
            if (state.IndexExists == false)
            {
                Rebuild();
            }

            if (state.IsBuilding)
            {
                if (state.LastVersionRequestTimebox.HasExpired)
                {
                    // TOOD: May be we need to signal a cancel for the prev run
                    BuildIndex(state.Id, new VersionRequestTimebox(DateTime.UtcNow), 2);
                }
            }
        }

        public void Rebuild() => Rebuild(2);

        public void Rebuild(int maxDegreeOfParallelism)
        {
            if (state.IsBuilding)
                return;

            BuildIndex(state.Id, new VersionRequestTimebox(DateTime.UtcNow), maxDegreeOfParallelism);
        }

        private void BuildIndex(EventStoreIndexManagerId id, VersionRequestTimebox timebox, int maxDegreeOfParallelism)
        {
            var @event = new EventStoreIndexRequested(id, DateTimeOffset.UtcNow, timebox, maxDegreeOfParallelism);
            Apply(@event);
        }

        public void FinalizeRequest()
        {
            if (state.IsBuilding)
            {
                var @event = new EventStoreIndexIsNowPresent(state.Id);
                Apply(@event);
            }
        }
    }
}
