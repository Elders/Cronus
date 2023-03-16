using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Snapshots
{
    public class SnapshotManagerState : AggregateRootState<SnapshotManager, SnapshotManagerId>
    {
        public override SnapshotManagerId Id { get; set; }

        public string AggregateContract { get; set; }

        public HashSet<RevisionStatus> Revisions { get; set; } = new();

        public RevisionStatus GetRevisionStatus(int revision) => Revisions.Where(x => x.Revision == revision).FirstOrDefault();

        public RevisionStatus LastRevision => Revisions
            .OrderByDescending(r => r.Revision)
            .FirstOrDefault();

        public RevisionStatus LastCompletedRevision => Revisions
            .Where(x => x.Status == SnapshotRevisionStatus.Completed)
            .MaxBy(x => x.Revision);

        public void When(SnapshotRequested e)
        {
            Id = e.Id;
            Revisions.Add(new RevisionStatus(e.Revision, SnapshotRevisionStatus.Running, e.Timestamp));
            AggregateContract = e.AggregateContract;
        }

        public void When(SnapshotCompleted e)
        {
            RevisionStatus rev = GetRevisionStatus(e.Revision);
            Revisions.Remove(rev);
            Revisions.Add(new RevisionStatus(e.Revision, SnapshotRevisionStatus.Completed, e.Timestamp));
        }

        public void When(SnapshotCanceled e)
        {
            RevisionStatus rev = GetRevisionStatus(e.Revision);
            Revisions.Remove(rev);
            Revisions.Add(new RevisionStatus(e.Revision, SnapshotRevisionStatus.Canceled, e.Timestamp));
        }

        public void When(SnapshotFailed e)
        {
            RevisionStatus rev = GetRevisionStatus(e.Revision);
            Revisions.Remove(rev);
            Revisions.Add(new RevisionStatus(e.Revision, SnapshotRevisionStatus.Failed, e.Timestamp));
        }
    }
}
