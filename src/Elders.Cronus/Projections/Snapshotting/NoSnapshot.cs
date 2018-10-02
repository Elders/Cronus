namespace Elders.Cronus.Projections.Snapshotting
{
    public class NoSnapshot : ISnapshot
    {
        public NoSnapshot(IBlobId id, string projectionName)
        {
            Id = id;
            ProjectionName = projectionName;
        }

        public IBlobId Id { get; set; }

        public string ProjectionName { get; set; }

        public int Revision { get { return 0; } }

        public object State { get; private set; }

        public void InitializeState(object state) { }

        public SnapshotMeta GetMeta()
        {
            return new SnapshotMeta(Revision, ProjectionName);
        }
    }
}
