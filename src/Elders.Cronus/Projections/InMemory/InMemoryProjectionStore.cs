using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.InMemory
{
    public class InMemoryProjectionStore : IProjectionStore
    {
        private readonly ConcurrentDictionary<ProjectionVersion, ConcurrentDictionary<IBlobId, List<ProjectionCommit>>> projectionCommits = new ConcurrentDictionary<ProjectionVersion, ConcurrentDictionary<IBlobId, List<ProjectionCommit>>>();

        public IEnumerable<ProjectionCommit> EnumerateProjection(ProjectionVersion version, IBlobId projectionId)
        {
            if (projectionCommits.ContainsKey(version) == false)
                return Enumerable.Empty<ProjectionCommit>();

            if (projectionCommits[version].ContainsKey(projectionId) == false)
                return Enumerable.Empty<ProjectionCommit>();

            return projectionCommits[version][projectionId];
        }

        public bool HasSnapshotMarker(ProjectionVersion version, IBlobId projectionId, int snapshotMarker)
        {
            return false; // We assume there are no snapshots in the InMemory implementation
        }

        public Task<bool> HasSnapshotMarkerAsync(ProjectionVersion version, IBlobId projectionId, int snapshotMarker)
        {
            return Task.FromResult(false); // We assume there are no snapshots in the InMemory implementation
        }

        public IEnumerable<ProjectionCommit> Load(ProjectionVersion version, IBlobId projectionId, int snapshotMarker)
        {
            if (projectionCommits.ContainsKey(version) == false)
                return Enumerable.Empty<ProjectionCommit>();

            if (projectionCommits[version].ContainsKey(projectionId) == false)
                return Enumerable.Empty<ProjectionCommit>();

            return projectionCommits[version][projectionId].Where(x => x.SnapshotMarker == snapshotMarker);
        }

        public Task<IEnumerable<ProjectionCommit>> LoadAsync(ProjectionVersion version, IBlobId projectionId, int snapshotMarker)
        {
            if (projectionCommits.ContainsKey(version) == false)
                return Task.FromResult(Enumerable.Empty<ProjectionCommit>());

            if (projectionCommits[version].ContainsKey(projectionId) == false)
                return Task.FromResult(Enumerable.Empty<ProjectionCommit>());

            var result = projectionCommits[version][projectionId].Where(x => x.SnapshotMarker == snapshotMarker);
            return Task.FromResult(result);
        }

        public void Save(ProjectionCommit commit)
        {
            if (projectionCommits.ContainsKey(commit.Version) == false)
                projectionCommits.TryAdd(commit.Version, new ConcurrentDictionary<IBlobId, List<ProjectionCommit>>());

            if (projectionCommits[commit.Version].ContainsKey(commit.ProjectionId) == false)
            {
                projectionCommits[commit.Version] = new ConcurrentDictionary<IBlobId, List<ProjectionCommit>>();
                projectionCommits[commit.Version].TryAdd(commit.ProjectionId, new List<ProjectionCommit>());
            }

            projectionCommits[commit.Version][commit.ProjectionId].Add(commit);
        }
    }
}
