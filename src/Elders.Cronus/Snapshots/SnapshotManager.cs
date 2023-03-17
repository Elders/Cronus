using System;
using System.Threading.Tasks;
using Elders.Cronus.Snapshots.Strategy;

namespace Elders.Cronus.Snapshots
{
    public class SnapshotManager : AggregateRoot<SnapshotManagerState>
    {
        public async Task RequestSnapshotAsync(SnapshotManagerId id, int revision, string contract, int eventsLoaded, TimeSpan loadTime, ISnapshotStrategy<AggregateSnapshotStrategyContext> snapshotStrategy)
        {
            if (state.LastRevision is not null && state.LastRevision.Status.IsRunning)
            {
                if (DateTimeOffset.UtcNow.AddSeconds(-30) > state.LastRevision.Timestamp)
                {
                    var now = DateTimeOffset.UtcNow;
                    var prevRevision = state.GetRevisionStatus(revision);
                    Apply(new SnapshotCanceled(id, prevRevision.Cancel(now), prevRevision, contract, now));
                }
                else
                    return;
            }

            var lastCompletedRevision = state.LastCompletedRevision ?? 0;
            var context = new AggregateSnapshotStrategyContext(id.InstanceId, lastCompletedRevision, revision, eventsLoaded, loadTime);
            var shouldRequestNewSnapshot = await snapshotStrategy.ShouldCreateSnapshotAsync(id, context).ConfigureAwait(false);
            if (shouldRequestNewSnapshot)
            {
                var now = DateTimeOffset.UtcNow;
                var runningRevision = new RevisionStatus(revision, SnapshotRevisionStatus.Running, now);
                Apply(new SnapshotRequested(id, runningRevision, contract, now));
            }
        }

        public void Complete(int revision)
        {
            var now = DateTimeOffset.UtcNow;
            var prevRevision = state.GetRevisionStatus(revision);
            Apply(new SnapshotCompleted(state.Id, prevRevision.Complete(now), prevRevision, state.Contract, now));
        }

        public void Cancel(int revision)
        {
            var now = DateTimeOffset.UtcNow;
            var prevRevision = state.GetRevisionStatus(revision);
            Apply(new SnapshotCanceled(state.Id, prevRevision.Cancel(now), prevRevision, state.Contract, now));
        }

        public void Fail(int revision)
        {
            var now = DateTimeOffset.UtcNow;
            var prevRevision = state.GetRevisionStatus(revision);
            Apply(new SnapshotFailed(state.Id, prevRevision.Fail(now), prevRevision, state.Contract, now));
        }
    }
}
