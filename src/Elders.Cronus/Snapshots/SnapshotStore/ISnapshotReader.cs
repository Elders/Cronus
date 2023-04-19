using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots.SnapshotStore
{
    public interface ISnapshotReader
    {
        Task<Snapshot> ReadAsync(IBlobId id);
        Task<Snapshot> ReadAsync(IBlobId id, int revision);
    }
}
