namespace Elders.Cronus.Projections.Snapshotting
{
    public class NoSnapshotStore : ISnapshotStore
    {
        public void InitializeProjectionSnapshotStore(ProjectionVersion version) { }

        public ISnapshot Load(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return new NoSnapshot(id, projectionName);
        }

        public SnapshotMeta LoadMeta(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return new NoSnapshot(id, projectionName).GetMeta();
        }

        public void Save(ISnapshot snapshot, ProjectionVersion version) { }
    }
}
