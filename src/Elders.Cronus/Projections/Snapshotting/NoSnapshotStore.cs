using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Snapshotting
{
    public class NoSnapshotStore : ISnapshotStore
    {
        public void InitializeProjectionSnapshotStore(ProjectionVersion version) { }

        public Task<ISnapshot> LoadAsync(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return Task.FromResult((ISnapshot)new NoSnapshot(id, projectionName));
        }

        public Task<SnapshotMeta> LoadMetaAsync(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return Task.FromResult(new NoSnapshot(id, projectionName).GetMeta());
        }

        public Task SaveAsync(ISnapshot snapshot, ProjectionVersion version) { return Task.CompletedTask; }
    }
}
