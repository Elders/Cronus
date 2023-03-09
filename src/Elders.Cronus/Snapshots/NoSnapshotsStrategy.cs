using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots
{
    public sealed class NoSnapshotsStrategy : ISnapshotStrategy
    {
        public Task<bool> ShouldCreateSnapshotAsync(SnapshotManagerId id, int lastCompletedRevision, int newRevision)
        {
            return Task.FromResult(false);
        }
    }

    public sealed class EveryEvenRevisionSnapshotsStrategy : ISnapshotStrategy
    {
        public Task<bool> ShouldCreateSnapshotAsync(SnapshotManagerId id, int lastCompletedRevision, int newRevision)
        {
            return Task.FromResult(newRevision % 2 == 0);
        }
    }
}
