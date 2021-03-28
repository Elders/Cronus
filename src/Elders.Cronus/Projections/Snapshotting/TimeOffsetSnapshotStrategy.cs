using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Projections.Snapshotting
{
    public class TimeOffsetSnapshotStrategy : EventsCountSnapshotStrategy
    {
        private readonly TimeSpan snapshotOffset;

        public TimeOffsetSnapshotStrategy(TimeSpan snapshotOffset, int eventsInSnapshot) : base(eventsInSnapshot)
        {
            this.snapshotOffset = snapshotOffset;
        }

        public override bool ShouldCreateSnapshot(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision)
        {
            return
                base.ShouldCreateSnapshot(commits, lastSnapshotRevision) ||
                commits.Select(x => DateTime.FromFileTimeUtc(x.EventOrigin.Timestamp)).DefaultIfEmpty(DateTime.MaxValue).Min() <= DateTime.UtcNow - snapshotOffset;
        }
    }
}
