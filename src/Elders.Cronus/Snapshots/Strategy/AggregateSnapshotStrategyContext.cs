using System;

namespace Elders.Cronus.Snapshots.Strategy
{
    public class AggregateSnapshotStrategyContext
    {
        public AggregateSnapshotStrategyContext(Urn aggregateId, int lastCompletedRevision, int aggregateRevision, int eventsLoaded, TimeSpan loadTime)
        {
            AggregateId = aggregateId;
            LastCompletedRevision = lastCompletedRevision;
            AggregateRevision = aggregateRevision;
            EventsLoaded = eventsLoaded;
            LoadTime = loadTime;
        }

        public Urn AggregateId { get; }
        public int LastCompletedRevision { get; }
        public int AggregateRevision { get; }
        public int EventsLoaded { get; }
        public TimeSpan LoadTime { get; }
    }
}
