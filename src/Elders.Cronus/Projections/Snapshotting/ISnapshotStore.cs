using System.Collections.Generic;

namespace Elders.Cronus.Projections.Snapshotting
{
    public interface ISnapshotStore
    {
        ISnapshot Load(string projectionName, IBlobId id, ProjectionVersion version);

        SnapshotMeta LoadMeta(string projectionName, IBlobId id, ProjectionVersion version);

        void Save(ISnapshot snapshot, ProjectionVersion version);

        void InitializeProjectionSnapshotStore(ProjectionVersion version);
    }

    public interface ISnapshotStoreFactory
    {
        ISnapshotStore GetSnapshotStore(string tenant);
        IEnumerable<ISnapshotStore> GetSnapshotStores();
    }
}
