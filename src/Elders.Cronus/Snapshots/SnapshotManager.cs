using System;
using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots
{
    public class SnapshotManager : AggregateRoot<SnapshotManagerState>
    {
        public async Task RequestSnapshotAsync(SnapshotManagerId id, int revision, string aggregateContract, ISnapshotStrategy snapshotStrategy)
        {
            if (state.LastRevision is not null && state.LastRevision.Status.IsRunning)
            {
                if (DateTimeOffset.UtcNow.AddSeconds(-30) > state.LastRevision.Timestamp)
                    Apply(new SnapshotCanceled(id, revision, aggregateContract, DateTimeOffset.UtcNow));
                else
                    return;
            }

            var lastCompletedRevision = state.LastCompletedRevision is null ? 0 : state.LastCompletedRevision.Revision;

            var shouldRequestNewSnapshot = await snapshotStrategy.ShouldCreateSnapshotAsync(id, lastCompletedRevision, revision).ConfigureAwait(false);
            if (shouldRequestNewSnapshot)
            {
                Apply(new SnapshotRequested(id, revision, aggregateContract, DateTimeOffset.UtcNow));
            }
        }

        public void MarkAsCreated(int revision)
        {
            Apply(new SnapshotCompleted(state.Id, revision, state.AggregateContract, DateTimeOffset.UtcNow));
        }

        public void MarkAsCanceled(int revision)
        {
            Apply(new SnapshotCanceled(state.Id, revision, state.AggregateContract, DateTimeOffset.UtcNow));
        }

        public void MarkAsFailed(int revision)
        {
            Apply(new SnapshotFailed(state.Id, revision, state.AggregateContract, DateTimeOffset.UtcNow));
        }
    }
}
