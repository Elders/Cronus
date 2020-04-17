using System.Collections.Generic;

namespace Elders.Cronus.Projections.Snapshotting
{
    public class NoSnapshotStrategy : ISnapshotStrategy
    {
        public int EventsInSnapshot => int.MaxValue;
        public int GetSnapshotMarker(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision) => 1;
        public bool ShouldCreateSnapshot(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision) => false;
    }
}
