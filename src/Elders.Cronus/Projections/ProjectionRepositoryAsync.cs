using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.Projections.Snapshotting;

namespace Elders.Cronus.Projections
{
    public partial class ProjectionRepository : IProjectionWriter, IProjectionReader
    {
        public async Task<ReadResult<T>> GetAsync<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

            try
            {
                Type projectionType = typeof(T);

                ProjectionStream stream = await LoadProjectionStreamAsync(projectionType, projectionId);
                return new ReadResult<T>(stream.RestoreFromHistory<T>());
            }
            catch (Exception ex)
            {
                log.ErrorException(ex, () => $"Unable to load projection. {typeof(T).Name}({projectionId})");
                return ReadResult<T>.WithError(ex);
            }
        }

        public async Task<ReadResult<IProjectionDefinition>> GetAsync(IBlobId projectionId, Type projectionType)
        {
            if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

            try
            {
                ProjectionStream stream = await LoadProjectionStreamAsync(projectionType, projectionId);
                return new ReadResult<IProjectionDefinition>(stream.RestoreFromHistory(projectionType));
            }
            catch (Exception ex)
            {
                log.ErrorException(ex, () => $"Unable to load projection. {projectionType.Name}({projectionId})");
                return ReadResult<IProjectionDefinition>.WithError(ex);
            }
        }

        async Task<ProjectionStream> LoadProjectionStreamAsync(Type projectionType, IBlobId projectionId)
        {
            string projectionName = projectionType.GetContractId();

            ReadResult<ProjectionVersions> result = GetProjectionVersions(projectionName);
            if (result.IsSuccess)
            {
                ProjectionVersion liveVersion = result.Data.GetLive();
                if (liveVersion is null)
                {
                    log.Warn(() => $"Unable to find projection `live` version. ProjectionId:{projectionId} ProjectionName:{projectionName} ProjectionType:{projectionType.Name}{Environment.NewLine}AvailableVersions:{Environment.NewLine}{result.Data.ToString()}");
                    return ProjectionStream.Empty();
                }

                ISnapshot snapshot = projectionType.IsSnapshotable()
                    ? await snapshotStore.LoadAsync(projectionName, projectionId, liveVersion).ConfigureAwait(false)
                    : new NoSnapshot(projectionId, projectionName);

                return await LoadProjectionStreamAsync(projectionType, liveVersion, projectionId, snapshot);
            }

            return ProjectionStream.Empty();
        }

        async Task<ProjectionStream> LoadProjectionStreamAsync(Type projectionType, ProjectionVersion version, IBlobId projectionId, ISnapshot snapshot)
        {
            Func<ISnapshot> loadSnapshot = () => snapshot;

            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();
            int snapshotMarker = snapshot.Revision;
            while (true)
            {
                snapshotMarker++;

                IEnumerable<ProjectionCommit> loadedProjectionCommits = await projectionStore.LoadAsync(version, projectionId, snapshotMarker);
                List<ProjectionCommit> loadedCommits = loadedProjectionCommits.ToList();
                projectionCommits.AddRange(loadedCommits);

                //if (projectionType.IsSnapshotable() && snapshotStrategy.ShouldCreateSnapshot(projectionCommits, snapshot.Revision))
                //{
                //    ProjectionStream checkpointStream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
                //    var projectionState = checkpointStream.RestoreFromHistory(projectionType).State;
                //    ISnapshot newSnapshot = new Snapshot(projectionId, version.ProjectionName, projectionState, snapshot.Revision + 1);
                //    snapshotStore.Save(newSnapshot, version);
                //    loadSnapshot = () => newSnapshot;

                //    projectionCommits.Clear();

                //    log.Debug(() => $"Snapshot created for projection `{version.ProjectionName}` with id={projectionId} where ({loadedCommits.Count}) were zipped. Snapshot: `{snapshot.GetType().Name}`");
                //}

                if (loadedCommits.Count < snapshotStrategy.EventsInSnapshot)
                    break;

                if (loadedCommits.Count > snapshotStrategy.EventsInSnapshot * 1.5)
                    log.Warn(() => $"Potential memory leak. The system will be down fairly soon. The projection `{version.ProjectionName}` with id={projectionId} loads a lot of projection commits ({loadedCommits.Count}) and snapshot `{snapshot.GetType().Name}` which puts a lot of CPU and RAM pressure. You can resolve this by configuring the snapshot settings`.");
            }

            ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
            return stream;
        }

    }
}
