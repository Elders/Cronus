using Elders.Cronus.MessageProcessing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Snapshotting
{
    public interface ISnapshotDistributor
    {
        Task<bool> CreateSnapshot(Type projectionType, ProjectionVersion version, IBlobId projectionId, string tenant);

        bool ShouldAttemptToCreateSnapshot(string snapshotData);
    }

    public class SnapshotDistributor : ISnapshotDistributor
    {
        private readonly ISnapshotStrategy snapshotStrategy;
        private readonly IProjectionStore projectionStore;
        private readonly ISnapshotStore snapshotStore;

        private readonly ConcurrentDictionary<string, int> snapshotsCount;

        public SnapshotDistributor(
            ISnapshotStore snapshotStore,
            ISnapshotStrategy snapshotStrategy,
            IProjectionStore projectionStore)
        {
            this.snapshotStrategy = snapshotStrategy;
            this.projectionStore = projectionStore;
            this.snapshotStore = snapshotStore;

            snapshotsCount = new ConcurrentDictionary<string, int>();
        }

        public async Task<bool> CreateSnapshot(Type projectionType, ProjectionVersion version, IBlobId projectionId, string tenant)
        {
            string projectionName = projectionType.GetContractId();
            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();

            SnapshotId snapshotId = new SnapshotId(projectionName, tenant);
            ISnapshot snapshot = await snapshotStore.LoadAsync(projectionName, snapshotId, version).ConfigureAwait(false);

            int snapshotMarker = snapshot.Revision;

            bool snapshotHasBeenCreated = false;
            bool shouldLoadMore = true;
            while (shouldLoadMore)
            {
                var loadProjectionCommits = projectionStore.LoadAsync(version, projectionId, snapshotMarker).ConfigureAwait(false);
                await foreach (var commit in loadProjectionCommits)
                    projectionCommits.Add(commit);

                ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, () => snapshot);

                if (projectionType.IsSnapshotable() && snapshotStrategy.ShouldCreateSnapshot(projectionCommits, snapshotMarker - 1))
                {
                    IProjectionDefinition projection = await stream.RestoreFromHistoryAsync(projectionType).ConfigureAwait(false);
                    ISnapshot newSnapshot = new Snapshot(snapshotId, projectionName, projection.State, snapshotMarker + 1);
                    await snapshotStore.SaveAsync(newSnapshot, version).ConfigureAwait(false);

                    projectionCommits.Clear();
                    snapshotHasBeenCreated = true;
                }

                shouldLoadMore = await projectionStore.HasSnapshotMarkerAsync(version, projectionId, snapshotMarker + 1).ConfigureAwait(false);
            }

            return snapshotHasBeenCreated;
        }

        public bool ShouldAttemptToCreateSnapshot(string snapshotData)
        {
            int count = snapshotsCount.AddOrUpdate(snapshotData, 1, (k, v) => v + 1);
            if (count >= snapshotStrategy.EventsInSnapshot)
            {
                snapshotsCount.Remove(snapshotData, out int _);
                return true;
            }

            return false;
        }
    }
}
