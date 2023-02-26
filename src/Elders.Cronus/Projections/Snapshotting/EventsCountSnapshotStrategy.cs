using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Projections.Snapshotting
{
    public class EventsCountSnapshotStrategy : ISnapshotStrategy
    {
        protected int eventsInSnapshot;

        public EventsCountSnapshotStrategy(int eventsInSnapshot)
        {
            if (eventsInSnapshot < 1) throw new ArgumentOutOfRangeException(nameof(eventsInSnapshot), $"{nameof(eventsInSnapshot)} must be greater than 0");

            this.eventsInSnapshot = eventsInSnapshot;
        }

        public int EventsInSnapshot { get { return eventsInSnapshot; } }

        public int GetSnapshotMarker(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision)
        {
            int snapshotMarker = commits.Select(x => x.SnapshotMarker).DefaultIfEmpty(lastSnapshotRevision + 1).Max();
            if (ShouldCreateSnapshot(commits, snapshotMarker))
                snapshotMarker++;

            return snapshotMarker;
        }

        public virtual bool ShouldCreateSnapshot(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision)
        {
            IEnumerable<ProjectionCommit> commitsAfterLastSnapshotRevision = commits.Where(x => x.SnapshotMarker >= lastSnapshotRevision);
            int latestSnapshotMarker = commitsAfterLastSnapshotRevision.Select(x => x.SnapshotMarker).DefaultIfEmpty(lastSnapshotRevision + 1).Max();
            if (latestSnapshotMarker > lastSnapshotRevision)
            {
                bool shouldCreateSnapshot = commitsAfterLastSnapshotRevision.Count() >= eventsInSnapshot;
                if (shouldCreateSnapshot)
                    return true;
            }

            return false;
        }
    }
}
