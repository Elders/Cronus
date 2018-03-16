using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Projections.Snapshotting
{
    public class DefaultSnapshotStrategy : ISnapshotStrategy
    {
        private TimeSpan snapshotOffset;
        private int eventsInSnapshot;

        public DefaultSnapshotStrategy(TimeSpan snapshotOffset, int eventsInSnapshot)
        {
            if (eventsInSnapshot < 1) throw new ArgumentOutOfRangeException(nameof(eventsInSnapshot), $"{nameof(eventsInSnapshot)} must be greater than 0");

            this.snapshotOffset = snapshotOffset;
            this.eventsInSnapshot = eventsInSnapshot;
        }

        public TimeSpan SnapshotOffset { get { return snapshotOffset; } }

        public int EventsInSnapshot { get { return eventsInSnapshot; } }

        public int GetSnapshotMarker(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision)
        {
            int snapshotMarker = commits.Select(x => x.SnapshotMarker).DefaultIfEmpty(lastSnapshotRevision + 1).Max();
            if (ShouldCreateSnapshot(commits, snapshotMarker))
                snapshotMarker++;

            return snapshotMarker;
        }

        public bool ShouldCreateSnapshot(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision)
        {
            var commitsAfterLastSnapshotRevision = commits.Where(x => x.SnapshotMarker > lastSnapshotRevision);
            int latestSnapshotMarker = commitsAfterLastSnapshotRevision.Select(x => x.SnapshotMarker).DefaultIfEmpty(lastSnapshotRevision + 1).Max();
            if (latestSnapshotMarker > lastSnapshotRevision)
            {
                bool shouldCreateSnapshot = commitsAfterLastSnapshotRevision.Count() >= eventsInSnapshot || commits.Select(x => x.TimeStamp).DefaultIfEmpty(DateTime.MaxValue).Min() <= DateTime.UtcNow - snapshotOffset;
                if (shouldCreateSnapshot)
                    return true;
            }

            return false;
        }
    }
}
