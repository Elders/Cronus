using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Snapshots
{
    public class SnapshotManagerState : AggregateRootState<SnapshotManager, SnapshotManagerId>
    {
        public override SnapshotManagerId Id { get; set; }

        public string AggregateContract { get; set; }

        public HashSet<RevisionStatus> Revisions { get; set; } = new();

        public RevisionStatus LastRevision => Revisions
            .OrderByDescending(r => r.Revision)
            .FirstOrDefault();

        public int LastCompletedRevision => Revisions
            .Where(x => x.Status == SnapshotStatus.Completed)
            .Select(x => x.Revision)
            .Max();

        public void When(SnapshotRequested e)
        {
            Id = e.Id;
            Revisions.Add(new RevisionStatus(e.Revision, SnapshotStatus.Running));
            AggregateContract = e.AggregareContract;
        }
    }
}
