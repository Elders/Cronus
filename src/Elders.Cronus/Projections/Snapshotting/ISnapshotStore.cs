namespace Elders.Cronus.Projections.Snapshotting
{
    public interface ISnapshotStore
    {
        ISnapshot Load(string projectionName, IBlobId id, ProjectionVersion version);

        void Save(ISnapshot snapshot, ProjectionVersion version);

        void InitializeProjectionSnapshotStore(ProjectionVersion version);
    }

    public class NoSnapshotStore : ISnapshotStore
    {
        public void InitializeProjectionSnapshotStore(ProjectionVersion version) { }

        public ISnapshot Load(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return new NoSnapshot(id, projectionName);
        }

        public void Save(ISnapshot snapshot, ProjectionVersion version) { }
    }
}
