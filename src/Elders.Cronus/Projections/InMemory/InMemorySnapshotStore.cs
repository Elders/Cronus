using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.Projections.Snapshotting;

namespace Elders.Cronus.Projections.InMemory
{
    public class InMemorySnapshotStore : ISnapshotStore
    {
        private readonly ConcurrentDictionary<ProjectionVersion, List<ISnapshot>> snapshots = new ConcurrentDictionary<ProjectionVersion, List<ISnapshot>>();

        public ISnapshot Load(string projectionName, IBlobId id, ProjectionVersion version)
        {
            if (snapshots.ContainsKey(version) == false)
                return new NoSnapshot(id, projectionName);

            var snapshot = snapshots[version].FirstOrDefault(x => x.ProjectionName == projectionName && x.Id == id);
            return snapshot ?? new NoSnapshot(id, projectionName);
        }

        public Task<ISnapshot> LoadAsync(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return Task.FromResult(Load(projectionName, id, version));
        }

        public SnapshotMeta LoadMeta(string projectionName, IBlobId id, ProjectionVersion version)
        {
            var snapshot = Load(projectionName, id, version);
            return SnapshotMeta.From(snapshot);
        }

        public Task<SnapshotMeta> LoadMetaAsync(string projectionName, IBlobId id, ProjectionVersion version)
        {
            return Task.FromResult(LoadMeta(projectionName, id, version));
        }

        public void Save(ISnapshot snapshot, ProjectionVersion version)
        {
            if (snapshots.ContainsKey(version) == false)
                snapshots.TryAdd(version, new List<ISnapshot>());

            snapshots[version].Add(snapshot);
        }
    }
}
