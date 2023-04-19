using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots.SnapshotStore
{
    public class NoOpSnapshotReader : ISnapshotReader
    {
        public Task<Snapshot> ReadAsync(IBlobId id) => Task.FromResult<Snapshot>(null);
        public Task<Snapshot> ReadAsync(IBlobId id, int revision) => Task.FromResult<Snapshot>(null);
    }
}
