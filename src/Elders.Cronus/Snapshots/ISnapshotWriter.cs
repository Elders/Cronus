using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots
{
    public interface ISnapshotWriter
    {
        Task WriteAsync(IBlobId id, int revision, object state);
    }

    public class NoOpSnapshotWriter : ISnapshotWriter
    {
        public Task WriteAsync(IBlobId id, int revision, object state) => Task.CompletedTask;
    }

    public interface ISnapshotReader
    {
        Task<Snapshot> ReadAsync(IBlobId id);
        Task<Snapshot> ReadAsync(IBlobId id, int revision);
    }

    public record Snapshot(IBlobId Id, int Revision, object State);

    public class NoOpSnapshotReader : ISnapshotReader
    {
        public Task<Snapshot> ReadAsync(IBlobId id) => Task.FromResult<Snapshot>(null);
        public Task<Snapshot> ReadAsync(IBlobId id, int revision) => Task.FromResult<Snapshot>(null);
    }
}
