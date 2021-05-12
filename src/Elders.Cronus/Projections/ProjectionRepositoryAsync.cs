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
            Type projectionType = typeof(T);

            using (log.BeginScope(s => s
                       .AddScope(Log.ProjectionName, projectionType.GetContractId())
                       .AddScope(Log.ProjectionType, projectionType.Name)
                       .AddScope(Log.ProjectionInstanceId, projectionId.RawId)))
            {
                if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

                try
                {
                    ProjectionStream stream = await LoadProjectionStreamAsync(projectionType, projectionId);
                    var readResult = new ReadResult<T>(stream.RestoreFromHistory<T>());
                    if (readResult.NotFound)
                        log.Warn(() => "Projection instance not found.");

                    return readResult;
                }
                catch (Exception ex)
                {
                    log.ErrorException(ex, () => $"Unable to load projection. {typeof(T).Name}({projectionId})");
                    return ReadResult<T>.WithError(ex);
                }
            }
        }

        public async Task<ReadResult<IProjectionDefinition>> GetAsync(IBlobId projectionId, Type projectionType)
        {
            using (log.BeginScope(s => s
                       .AddScope(Log.ProjectionName, projectionType.GetContractId())
                       .AddScope(Log.ProjectionType, projectionType.Name)
                       .AddScope(Log.ProjectionInstanceId, projectionId.RawId)))
            {
                if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

                try
                {
                    ProjectionStream stream = await LoadProjectionStreamAsync(projectionType, projectionId);
                    var readResult = new ReadResult<IProjectionDefinition>(stream.RestoreFromHistory(projectionType));
                    if (readResult.NotFound)
                        log.Warn(() => "Projection instance not found.");

                    return readResult;
                }
                catch (Exception ex)
                {
                    log.ErrorException(ex, () => $"Unable to load projection. {projectionType.Name}({projectionId})");
                    return ReadResult<IProjectionDefinition>.WithError(ex);
                }
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
            bool shouldLoadMore = true;
            Func<ISnapshot> loadSnapshot = () => snapshot;

            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();
            int snapshotMarker = snapshot.Revision;
            while (shouldLoadMore)
            {
                snapshotMarker++;

                var loadProjectionCommitsTask = projectionStore.LoadAsync(version, projectionId, snapshotMarker).ConfigureAwait(false);
                var checkNextSnapshotMarkerTask = projectionStore.HasSnapshotMarkerAsync(version, projectionId, snapshotMarker + 1).ConfigureAwait(false);

                IEnumerable<ProjectionCommit> loadedProjectionCommits = await loadProjectionCommitsTask;
                shouldLoadMore = await checkNextSnapshotMarkerTask;

                projectionCommits.AddRange(loadedProjectionCommits);
            }

            ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
            return stream;
        }

    }
}
