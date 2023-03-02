using System.Threading.Tasks;

namespace Elders.Cronus.Snapshots
{
    public interface ISnapshotWriter
    {
        Task WriteAsync(Urn id, int revision, object state);
    }

    public class NoOpSnapshotWriter : ISnapshotWriter
    {
        public Task WriteAsync(Urn id, int revision, object state) => Task.CompletedTask;
    }

    public interface ISnapshotReader
    {
        Task<Snapshot> ReadAsync(Urn id);
        Task<Snapshot> ReadAsync(Urn id, int revision);
    }

    public record Snapshot(Urn Id, int Revision, object State);

    public class NoOpSnapshotReader : ISnapshotReader
    {
        public Task<Snapshot> ReadAsync(Urn id) => Task.FromResult<Snapshot>(null);
        public Task<Snapshot> ReadAsync(Urn id, int revision) => Task.FromResult<Snapshot>(null);
    }
}
