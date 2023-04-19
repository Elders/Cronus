using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Snapshots
{
    public class SnapshotManagerState : AggregateRootState<SnapshotManager, SnapshotManagerId>
    {
        public override SnapshotManagerId Id { get; set; }

        public string Contract { get; set; }

        public HashSet<RevisionStatus> Revisions { get; set; } = new();

        public RevisionStatus GetRevisionStatus(int revision) => Revisions.Where(x => x.Revision == revision).FirstOrDefault();

        public RevisionStatus LastRevision => Revisions
            .MaxBy(r => r.Revision);

        public int? LastCompletedRevision => Revisions
            .Where(x => x.Status == SnapshotRevisionStatus.Completed)
            .MaxBy(x => x.Revision)
            ?.Revision;

        public void When(SnapshotRequested e)
        {
            Id = e.Id;
            Revisions.Add(e.NewRevisionStatus);
            Contract = e.Contract;
        }

        public void When(SnapshotCompleted e)
        {
            Revisions.Remove(e.PreviousRevisionStatus);
            Revisions.Add(e.NewRevisionStatus);
        }

        public void When(SnapshotCanceled e)
        {
            Revisions.Remove(e.PreviousRevisionStatus);
            Revisions.Add(e.NewRevisionStatus);
        }

        public void When(SnapshotFailed e)
        {
            Revisions.Remove(e.PreviousRevisionStatus);
            Revisions.Add(e.NewRevisionStatus);
        }
    }
}
