using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots.SnapshotStore
{
    public interface ISnapshotWriter
    {
        Task WriteAsync(IBlobId id, int revision, object state);
    }
}
