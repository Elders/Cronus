using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Snapshotting
{
    public interface ISnapshotStore
    {
        Task<ISnapshot> LoadAsync(string projectionName, IBlobId id, ProjectionVersion version);

        Task<SnapshotMeta> LoadMetaAsync(string projectionName, IBlobId id, ProjectionVersion version);

        Task SaveAsync(ISnapshot snapshot, ProjectionVersion version);
    }

    public interface ISnapshotStoreFactory
    {
        ISnapshotStore GetSnapshotStore(string tenant);
        IEnumerable<ISnapshotStore> GetSnapshotStores();
    }
}
