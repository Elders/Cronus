using System;
using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots
{
    public class SnapshotManager : AggregateRoot<SnapshotManagerState>
    {
        public async Task RequestSnapshotAsync(SnapshotManagerId id, int revision, string aggregateContract, ISnapshotStrategy snapshotStrategy)
        {
            if (state.LastRevision is not null && state.LastRevision.Status.IsRunning)
                return;

            var shouldRequestNewSnapshot = await snapshotStrategy.ShouldCreateSnapshotAsync(id, state.LastCompletedRevision, revision).ConfigureAwait(false);
            if (shouldRequestNewSnapshot)
            {
                Apply(new SnapshotRequested(id, revision, aggregateContract, DateTimeOffset.UtcNow));
            }
        }
    }
}
