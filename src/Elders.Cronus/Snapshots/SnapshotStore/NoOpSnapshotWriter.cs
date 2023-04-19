using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots.SnapshotStore
{
    public class NoOpSnapshotWriter : ISnapshotWriter
    {
        public Task WriteAsync(IBlobId id, int revision, object state) => Task.CompletedTask;
    }
}
