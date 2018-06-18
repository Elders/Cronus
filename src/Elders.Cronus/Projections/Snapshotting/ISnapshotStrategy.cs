using System;
using System.Collections.Generic;

namespace Elders.Cronus.Projections.Snapshotting
{
    public interface ISnapshotStrategy
    {
        int EventsInSnapshot { get; }

        int GetSnapshotMarker(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision);
        bool ShouldCreateSnapshot(IEnumerable<ProjectionCommit> commits, int lastSnapshotRevision);
    }
}
