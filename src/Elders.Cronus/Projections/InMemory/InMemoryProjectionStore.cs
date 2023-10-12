//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Elders.Cronus.Projections.InMemory
//{
//    public class InMemoryProjectionStore : IProjectionStore
//    {
//        private readonly ConcurrentDictionary<ProjectionVersion, ConcurrentDictionary<IBlobId, List<ProjectionCommit>>> projectionCommits = new ConcurrentDictionary<ProjectionVersion, ConcurrentDictionary<IBlobId, List<ProjectionCommit>>>();

//        public async IAsyncEnumerable<ProjectionCommit> EnumerateProjectionAsync(ProjectionVersion version, IBlobId projectionId)
//        {
//            if (projectionCommits.ContainsKey(version) == false)
//                yield break;

//            if (projectionCommits[version].ContainsKey(projectionId) == false)
//                yield break;

//            foreach (var commit in projectionCommits[version][projectionId])
//                yield return commit;
//        }

//        public async IAsyncEnumerable<ProjectionCommit> LoadAsync(ProjectionVersion version, IBlobId projectionId)
//        {
//            if (projectionCommits.ContainsKey(version) == false)
//                yield break;

//            if (projectionCommits[version].ContainsKey(projectionId) == false)
//                yield break;

//            IEnumerable<ProjectionCommit> commits = projectionCommits[version][projectionId];
//            foreach (var commit in commits)
//                yield return commit;
//        }

//        public async Task SaveAsync(ProjectionCommit commit)
//        {
//            if (projectionCommits.ContainsKey(commit.Version) == false)
//                projectionCommits.TryAdd(commit.Version, new ConcurrentDictionary<IBlobId, List<ProjectionCommit>>());

//            if (projectionCommits[commit.Version].ContainsKey(commit.ProjectionId) == false)
//            {
//                projectionCommits[commit.Version] = new ConcurrentDictionary<IBlobId, List<ProjectionCommit>>();
//                projectionCommits[commit.Version].TryAdd(commit.ProjectionId, new List<ProjectionCommit>());
//            }

//            projectionCommits[commit.Version][commit.ProjectionId].Add(commit);

//            await Task.CompletedTask;
//        }
//    }
//}
