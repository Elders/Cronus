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

        public Task<ISnapshot> LoadAsync(string projectionName, IBlobId id, ProjectionVersion version)
        {
            if (snapshots.ContainsKey(version) == false)
                return Task.FromResult((ISnapshot)new NoSnapshot(id, projectionName));

            ISnapshot snapshot = snapshots[version].FirstOrDefault(x => x.ProjectionName == projectionName && x.Id == id);
            return Task.FromResult(snapshot ?? new NoSnapshot(id, projectionName));
        }

        public async Task<SnapshotMeta> LoadMetaAsync(string projectionName, IBlobId id, ProjectionVersion version)
        {
            var snapshot = await LoadAsync(projectionName, id, version).ConfigureAwait(false);
            return SnapshotMeta.From(snapshot);
        }

        public Task SaveAsync(ISnapshot snapshot, ProjectionVersion version)
        {
            if (snapshots.ContainsKey(version) == false)
                snapshots.TryAdd(version, new List<ISnapshot>());

            snapshots[version].Add(snapshot);

            return Task.CompletedTask;
        }
    }
}
