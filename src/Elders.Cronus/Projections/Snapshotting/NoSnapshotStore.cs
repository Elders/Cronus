using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Snapshotting
{
    public class NoSnapshotStore : ISnapshotStore
    {
        public void InitializeProjectionSnapshotStore(ProjectionVersion version) { }

        public ISnapshot Load(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return new NoSnapshot(id, projectionName);
        }

        public Task<ISnapshot> LoadAsync(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return Task.FromResult(Load(projectionName, id, version));
        }

        public SnapshotMeta LoadMeta(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return new NoSnapshot(id, projectionName).GetMeta();
        }

        public Task<SnapshotMeta> LoadMetaAsync(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return Task.FromResult(LoadMeta(projectionName, id, version));
        }

        public void Save(ISnapshot snapshot, ProjectionVersion version) { }
    }
}
