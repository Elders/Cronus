using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Snapshotting;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections
{
    public partial class ProjectionRepository : IProjectionWriter, IProjectionReader
    {
        private static readonly ILogger log = CronusLogger.CreateLogger(typeof(ProjectionRepository));

        private readonly ICronusContextAccessor contextAccessor;
        readonly IProjectionStore projectionStore;
        readonly ISnapshotStore snapshotStore;
        readonly ISnapshotStrategy snapshotStrategy;
        private readonly IHandlerFactory handlerFactory;
        private readonly ProjectionHasher projectionHasher;

        public ProjectionRepository(ICronusContextAccessor contextAccessor, IProjectionStore projectionStore, ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy, IHandlerFactory handlerFactory, ProjectionHasher projectionHasher)
        {
            if (contextAccessor is null) throw new ArgumentException(nameof(contextAccessor));
            if (projectionStore is null) throw new ArgumentException(nameof(projectionStore));
            if (snapshotStore is null) throw new ArgumentException(nameof(snapshotStore));
            if (snapshotStrategy is null) throw new ArgumentException(nameof(snapshotStrategy));

            this.contextAccessor = contextAccessor;
            this.projectionStore = projectionStore;
            this.snapshotStore = snapshotStore;
            this.snapshotStrategy = snapshotStrategy;
            this.handlerFactory = handlerFactory;
            this.projectionHasher = projectionHasher;
        }

        public async Task SaveAsync(Type projectionType, CronusMessage cronusMessage)
        {
            if (projectionType is null) throw new ArgumentNullException(nameof(projectionType));
            if (cronusMessage is null) throw new ArgumentNullException(nameof(cronusMessage));

            EventOrigin eventOrigin = cronusMessage.GetEventOrigin();
            IEvent @event = cronusMessage.Payload as IEvent;

            await SaveAsync(projectionType, @event, eventOrigin).ConfigureAwait(false); ;
        }

        public async Task SaveAsync(Type projectionType, IEvent @event, EventOrigin eventOrigin)
        {
            if (projectionType is null) throw new ArgumentNullException(nameof(projectionType));
            if (@event is null) throw new ArgumentNullException(nameof(@event));
            if (eventOrigin is null) throw new ArgumentNullException(nameof(eventOrigin));

            string projectionName = projectionType.GetContractId();
            var handlerInstance = handlerFactory.Create(projectionType);
            var projection = handlerInstance.Current as IProjectionDefinition;
            if (projection != null)
            {
                var projectionIds = projection.GetProjectionIds(@event);

                List<Task> tasks = new List<Task>();
                foreach (IBlobId projectionId in projectionIds)
                {
                    using (log.BeginScope(s => s.AddScope("cronus_projection_id", projectionId)))
                    {
                        ReadResult<ProjectionVersions> result = await GetProjectionVersionsAsync(projectionName).ConfigureAwait(false);
                        if (result.IsSuccess)
                        {
                            foreach (ProjectionVersion version in result.Data)
                            {
                                using (log.BeginScope(s => s.AddScope("cronus_projection_version", version)))
                                {
                                    if (ShouldSaveEventForVersion(version))
                                    {
                                        Task task = PersistAsync(projectionType, projectionName, projectionId, version, @event, eventOrigin);
                                        tasks.Add(task);
                                    }
                                }
                            }
                        }
                        if (result.HasError)
                        {
                            log.Error(() => "Failed to update projection because the projection version failed to load. Please replay the projection to restore the state. Self-heal hint!" + Environment.NewLine + result.Error + Environment.NewLine + $"\tProjectionName:{projectionName}" + Environment.NewLine + $"\tEvent:{@event}");
                        }
                    }
                }
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        // Used by replay projections only
        public async Task SaveAsync(Type projectionType, IEvent @event, EventOrigin eventOrigin, ProjectionVersion version)
        {
            if (projectionType is null) throw new ArgumentNullException(nameof(projectionType));
            if (@event is null) throw new ArgumentNullException(nameof(@event));
            if (version is null) throw new ArgumentNullException(nameof(version));

            if (ShouldSaveEventForVersion(version) == false)
                throw new ArgumentException("Invalid version. Only versions in `Building` and `Live` status are eligable for persistence.", nameof(version));

            string projectionName = projectionType.GetContractId();
            if (projectionName.Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase) == false)
                throw new ArgumentException($"Invalid version. The version `{version}` does not match projection `{projectionName}`", nameof(version));

            bool isProjectionDefinitionType = typeof(IProjectionDefinition).IsAssignableFrom(projectionType);
            bool isEventSourcedType = typeof(IAmEventSourcedProjection).IsAssignableFrom(projectionType);

            if (isProjectionDefinitionType)
            {
                var handlerInstance = handlerFactory.Create(projectionType);
                var projection = handlerInstance.Current as IProjectionDefinition;

                var projectionIds = projection.GetProjectionIds(@event);
                List<Task> tasks = new List<Task>();
                foreach (var projectionId in projectionIds)
                {
                    Task task = PersistAsync(projectionType, projectionName, projectionId, version, @event, eventOrigin);
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            else if (isEventSourcedType)
            {
                try
                {
                    var projectionId = Urn.Parse($"urn:cronus:{projectionName}");

                    var commit = new ProjectionCommit(projectionId, version, @event, 1, eventOrigin, DateTime.FromFileTime(eventOrigin.Timestamp)); // Snapshotting is disable till we test it => hardcoded 1
                    await projectionStore.SaveAsync(commit).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    log.ErrorException(ex, () => "Failed to persist event." + Environment.NewLine + $"\tProjectionVersion:{version}" + Environment.NewLine + $"\tEvent:{@event}");
                }
            }
        }

        public async Task<ReadResult<T>> GetAsync<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            Type projectionType = typeof(T);

            using (log.BeginScope(s => s
                       .AddScope(Log.ProjectionName, projectionType.GetContractId())
                       .AddScope(Log.ProjectionType, projectionType.Name)
                       .AddScope(Log.ProjectionInstanceId, projectionId.RawId)))
            {
                if (projectionId is null) throw new ArgumentNullException(nameof(projectionId));

                try
                {
                    ProjectionStream stream = await LoadProjectionStreamAsync(projectionType, projectionId).ConfigureAwait(false);
                    var readResult = new ReadResult<T>(await stream.RestoreFromHistoryAsync<T>().ConfigureAwait(false));
                    if (readResult.NotFound && log.IsDebugEnabled())
                        log.Debug(() => "Projection instance not found.");

                    return readResult;
                }
                catch (Exception ex)
                {
                    log.ErrorException(ex, () => "Unable to load projection.");
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
                if (projectionId is null) throw new ArgumentNullException(nameof(projectionId));

                try
                {
                    ProjectionStream stream = await LoadProjectionStreamAsync(projectionType, projectionId);
                    var readResult = new ReadResult<IProjectionDefinition>(await stream.RestoreFromHistoryAsync(projectionType).ConfigureAwait(false));
                    if (readResult.NotFound && log.IsDebugEnabled())
                        log.Debug(() => "Projection instance not found.");

                    return readResult;
                }
                catch (Exception ex)
                {
                    log.ErrorException(ex, () => "Unable to load projection.");
                    return ReadResult<IProjectionDefinition>.WithError(ex);
                }
            }
        }

        protected async virtual Task<ReadResult<ProjectionVersions>> GetProjectionVersionsAsync(string projectionName)
        {
            if (string.IsNullOrEmpty(projectionName)) throw new ArgumentNullException(nameof(projectionName));

            var queryResult = await GetProjectionVersionsFromStoreAsync(projectionName).ConfigureAwait(false);
            if (queryResult.IsSuccess)
            {
                return new ReadResult<ProjectionVersions>(queryResult.Data.State.AllVersions);
            }

            if (queryResult.HasError || queryResult.NotFound)
                return ReadResult<ProjectionVersions>.WithError(queryResult.Error);

            return default;
        }

        private async Task PersistAsync(Type projectionType, string projectionName, IBlobId projectionId, ProjectionVersion version, IEvent @event, EventOrigin eventOrigin)
        {
            try
            {
                SnapshotMeta snapshotMeta = await GetSnapshotMeta(projectionType, projectionName, projectionId, version).ConfigureAwait(false);
                int snapshotMarker = snapshotMeta.Revision == 0 ? 1 : snapshotMeta.Revision + 2;

                var commit = new ProjectionCommit(projectionId, version, @event, snapshotMarker, eventOrigin, DateTime.FromFileTime(eventOrigin.Timestamp));
                await projectionStore.SaveAsync(commit).ConfigureAwait(false);
            }
            catch (Exception ex) when (LogMessageWhenFailToUpdateProjection(ex, version)) { }
        }

        private async Task<ReadResult<ProjectionVersionsHandler>> GetProjectionVersionsFromStoreAsync(string projectionName)
        {
            try
            {
                var persistentVersionType = typeof(ProjectionVersionsHandler);
                var projectionVersions_ProjectionName = persistentVersionType.GetContractId();

                var versionId = new ProjectionVersionManagerId(projectionName, contextAccessor.CronusContext.Tenant);
                var persistentVersion = new ProjectionVersion(projectionVersions_ProjectionName, ProjectionStatus.Live, 1, projectionHasher.CalculateHash(persistentVersionType));
                ProjectionStream stream = await LoadProjectionStreamAsync(persistentVersionType, persistentVersion, versionId, new NoSnapshot(versionId, projectionVersions_ProjectionName).GetMeta()).ConfigureAwait(false);
                var queryResult = await stream.RestoreFromHistoryAsync<ProjectionVersionsHandler>().ConfigureAwait(false);

                return new ReadResult<ProjectionVersionsHandler>(queryResult);
            }
            catch (Exception ex)
            {
                log.WarnException(ex, () => $"Failed to load projection versions. ProjectionName: {projectionName}");
                return ReadResult<ProjectionVersionsHandler>.WithError(ex.Message);
            }
        }

        private async Task<ProjectionStream> LoadProjectionStreamAsync(Type projectionType, ProjectionVersion version, IBlobId projectionId, SnapshotMeta snapshotMeta)
        {
            ISnapshot loadedSnapshot = await snapshotStore.LoadAsync(version.ProjectionName, projectionId, version).ConfigureAwait(false);

            Func<ISnapshot> loadSnapshot = () => projectionType.IsSnapshotable()
                ? loadedSnapshot
                : new NoSnapshot(projectionId, version.ProjectionName);

            return await LoadProjectionStreamAsync(version, projectionId, snapshotMeta, loadSnapshot).ConfigureAwait(false);
        }

        private async Task<ProjectionStream> LoadProjectionStreamAsync(ProjectionVersion version, IBlobId projectionId, SnapshotMeta snapshotMeta, Func<ISnapshot> loadSnapshot)
        {
            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();
            int snapshotMarker = snapshotMeta.Revision;

            bool shouldLoadMore = true;
            while (shouldLoadMore)
            {
                snapshotMarker++;
                var loadedCommits = projectionStore.LoadAsync(version, projectionId, snapshotMarker).ConfigureAwait(false);
                await foreach (var commit in loadedCommits)
                {
                    projectionCommits.Add(commit);
                }

                shouldLoadMore = await projectionStore.HasSnapshotMarkerAsync(version, projectionId, snapshotMarker + 1).ConfigureAwait(false);
            }

            ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
            return stream;
        }

        private async Task<ProjectionStream> LoadProjectionStreamAsync(ProjectionVersion version, IBlobId projectionId, ISnapshot snapshot)
        {
            bool shouldLoadMore = true;
            Func<ISnapshot> loadSnapshot = () => snapshot;

            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();
            int snapshotMarker = snapshot.Revision;
            while (shouldLoadMore)
            {
                snapshotMarker++;

                var loadProjectionCommits = projectionStore.LoadAsync(version, projectionId, snapshotMarker).ConfigureAwait(false);
                bool checkNextSnapshotMarker = await projectionStore.HasSnapshotMarkerAsync(version, projectionId, snapshotMarker + 1).ConfigureAwait(false);
                shouldLoadMore = checkNextSnapshotMarker;

                await foreach (var commit in loadProjectionCommits)
                    projectionCommits.Add(commit);
            }

            ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
            return stream;
        }

        private async Task<ProjectionStream> LoadProjectionStreamAsync(Type projectionType, IBlobId projectionId)
        {
            string projectionName = projectionType.GetContractId();

            ReadResult<ProjectionVersions> result = await GetProjectionVersionsAsync(projectionName).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                ProjectionVersion liveVersion = result.Data.GetLive();
                if (liveVersion is null)
                {
                    log.Warn(() => $"Unable to find projection `live` version. {Environment.NewLine}AvailableVersions:{Environment.NewLine}{result.Data.ToString()}");
                    return ProjectionStream.Empty();
                }

                ISnapshot snapshot = projectionType.IsSnapshotable()
                    ? await snapshotStore.LoadAsync(projectionName, projectionId, liveVersion).ConfigureAwait(false)
                    : new NoSnapshot(projectionId, projectionName);

                return await LoadProjectionStreamAsync(liveVersion, projectionId, snapshot);
            }

            return ProjectionStream.Empty();
        }

        private bool LogMessageWhenFailToUpdateProjection(Exception ex, ProjectionVersion version)
        {
            if (version.Status == ProjectionStatus.Live)
                log.ErrorException(ex, () => $"Failed to update projection. Please replay the projection to restore the state.");
            else
                log.WarnException(ex, () => $"Failed to update projection. Most probably the event will be replayed later so you could ignore this message OR log a bug.");

            return true;
        }

        private bool ShouldSaveEventForVersion(ProjectionVersion version)
        {
            return version.Status == ProjectionStatus.Building || version.Status == ProjectionStatus.Replaying || version.Status == ProjectionStatus.Rebuilding || version.Status == ProjectionStatus.Live;
        }

        private async Task<SnapshotMeta> GetSnapshotMeta(Type projectionType, string projectionName, IBlobId projectionId, ProjectionVersion version)
        {
            SnapshotMeta snapshotMeta;
            if (projectionType.IsSnapshotable())
                snapshotMeta = await snapshotStore.LoadMetaAsync(projectionName, projectionId, version).ConfigureAwait(false);
            else
                snapshotMeta = new NoSnapshot(projectionId, projectionName).GetMeta();

            return snapshotMeta;
        }
    }
}
