namespace Elders.Cronus.Projections.Snapshotting
{
    public interface ISnapshotStore
    {
        ISnapshot Load(string contractId, IBlobId id, ProjectionVersion projectionVersion);

        void Save(ISnapshot snapshot, ProjectionVersion projectionVersion);

        void InitializeProjectionSnapshotStore(ProjectionVersion projectionVersion);
    }

    public class NoSnapshotStore : ISnapshotStore
    {
        public void InitializeProjectionSnapshotStore(ProjectionVersion projectionVersion) { }

        public ISnapshot Load(string projectionContractId, IBlobId id, ProjectionVersion projectionVersion)
        {
            return new NoSnapshot(id, projectionContractId);
        }

        public void Save(ISnapshot snapshot, ProjectionVersion projectionVersion) { }
    }
}
