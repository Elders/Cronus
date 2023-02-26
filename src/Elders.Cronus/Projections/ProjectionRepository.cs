using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Snapshotting;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections
{
    public partial class ProjectionRepository : IProjectionWriter, IProjectionReader
    {
        private readonly ILogger logger;
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
        private static long LastRefreshTimestamp = 0;

        private readonly CronusContext context;
        private readonly ProjectionHasher projectionHasher;
        readonly InMemoryProjectionVersionStore inMemoryVersionStore;
        readonly IProjectionStore projectionStore;
        readonly ISnapshotStore snapshotStore;
        private readonly IHandlerFactory handlerFactory;
        private readonly ISnapshotDistributor distibutor;
        private readonly IPublisher<ISystemSignal> signalPublisher;

        public ProjectionRepository(CronusContext context,
            IProjectionStore projectionStore,
            ISnapshotStore snapshotStore,
            ISnapshotStrategy snapshotStrategy,
            IHandlerFactory handlerFactory,
            IPublisher<ISystemSignal> signalPublisher,
            InMemoryProjectionVersionStore inMemoryVersionStore,
            ProjectionHasher projectionHasher,
            ISnapshotDistributor distibutor,
            ILogger<ProjectionRepository> logger)
        {
            if (context is null) throw new ArgumentException(nameof(context));
            if (projectionStore is null) throw new ArgumentException(nameof(projectionStore));
            if (snapshotStore is null) throw new ArgumentException(nameof(snapshotStore));
            if (snapshotStrategy is null) throw new ArgumentException(nameof(snapshotStrategy));
            if (inMemoryVersionStore is null) throw new ArgumentException(nameof(inMemoryVersionStore));

            this.context = context;
            this.projectionStore = projectionStore;
            this.snapshotStore = snapshotStore;
            this.inMemoryVersionStore = inMemoryVersionStore;
            this.handlerFactory = handlerFactory;
            this.projectionHasher = projectionHasher;
            this.signalPublisher = signalPublisher;
            this.logger = logger;
            this.distibutor = distibutor;
        }

        public async Task SaveAsync(Type projectionType, CronusMessage cronusMessage)
        {
            if (ReferenceEquals(null, projectionType)) throw new ArgumentNullException(nameof(projectionType));
            if (ReferenceEquals(null, cronusMessage)) throw new ArgumentNullException(nameof(cronusMessage));

            EventOrigin eventOrigin = cronusMessage.GetEventOrigin();
            IEvent @event = cronusMessage.Payload as IEvent;

            await SaveAsync(projectionType, @event, eventOrigin).ConfigureAwait(false); ;
        }

        public async Task SaveAsync(Type projectionType, IEvent @event, EventOrigin eventOrigin)
        {
            if (ReferenceEquals(null, projectionType)) throw new ArgumentNullException(nameof(projectionType));
            if (ReferenceEquals(null, @event)) throw new ArgumentNullException(nameof(@event));
            if (ReferenceEquals(null, eventOrigin)) throw new ArgumentNullException(nameof(eventOrigin));

            string projectionName = projectionType.GetContractId();
            var handlerInstance = handlerFactory.Create(projectionType);
            var projection = handlerInstance.Current as IProjectionDefinition;
            if (projection != null)
            {
                var projectionIds = projection.GetProjectionIds(@event);

                foreach (var projectionId in projectionIds)
                {
                    using (logger.BeginScope(s => s.AddScope("cronus_projection_id", projectionId)))
                    {
                        ReadResult<ProjectionVersions> result = await GetProjectionVersionsAsync(projectionName).ConfigureAwait(false);
                        if (result.IsSuccess)
                        {
                            foreach (ProjectionVersion version in result.Data)
                            {
                                if (ShouldSaveEventForVersion(version) == false)
                                    continue;

                                using (logger.BeginScope(s => s.AddScope("cronus_projection_version", version)))
                                {
                                    try
                                    {
                                        int snapshotMarker = -1;

                                        if (projectionType.IsSnapshotable())
                                        {
                                            SnapshotId snapshotId = new SnapshotId(projectionName, context.Tenant);
                                            SnapshotMeta snapshotMeta = await snapshotStore.LoadMetaAsync(projectionName, snapshotId, version).ConfigureAwait(false);
                                            snapshotMarker = snapshotMeta.Revision;

                                            string snapData = $"{snapshotId}:{snapshotMeta.Revision}";
                                            if (distibutor.ShouldAttemptToCreateSnapshot(snapData))
                                            {
                                                var commitSignal = new ProjectionCommitSignaled(context.Tenant, projectionId, projectionType, version);
                                                signalPublisher.Publish(commitSignal);
                                            }
                                        }

                                        ProjectionCommit commit = new ProjectionCommit(projectionId, projectionType, version, @event, snapshotMarker, eventOrigin, DateTime.UtcNow);
                                        await projectionStore.SaveAsync(commit).ConfigureAwait(false);
                                    }
                                    catch (Exception ex) when (LogMessageWhenFailToUpdateProjection(ex, version)) { }
                                }
                            }

                            if (result.HasError)
                            {
                                logger.Error(() => "Failed to update projection because the projection version failed to load. Please replay the projection to restore the state. Self-heal hint!" + Environment.NewLine + result.Error + Environment.NewLine + $"\tProjectionName:{projectionName}" + Environment.NewLine + $"\tEvent:{@event}");
                            }
                        }
                    }
                }
            }
        }

        // Used by replay projections only
        public async Task SaveAsync(Type projectionType, IEvent @event, EventOrigin eventOrigin, ProjectionVersion version)
        {
            if (ReferenceEquals(null, projectionType)) throw new ArgumentNullException(nameof(projectionType));
            if (ReferenceEquals(null, @event)) throw new ArgumentNullException(nameof(@event));
            if (ReferenceEquals(null, eventOrigin)) throw new ArgumentNullException(nameof(eventOrigin));
            if (ReferenceEquals(null, version)) throw new ArgumentNullException(nameof(version));

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

                foreach (var projectionId in projectionIds)
                {
                    try
                    {
                        SnapshotId snapshotId = new SnapshotId(projectionName, context.Tenant);
                        SnapshotMeta snapshotMeta = await snapshotStore.LoadMetaAsync(projectionName, snapshotId, version).ConfigureAwait(false);
                        ProjectionCommit commit = new ProjectionCommit(projectionId, projectionType, version, @event, snapshotMeta.Revision, eventOrigin, DateTime.UtcNow);

                        await projectionStore.SaveAsync(commit).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(ex, () => "Failed to persist event." + Environment.NewLine + $"\tProjectionVersion:{version}" + Environment.NewLine + $"\tEvent:{@event}");
                    }
                }
            }
            else if (isEventSourcedType)
            {
                try
                {
                    var projectionId = Urn.Parse($"urn:cronus:{projectionName}");

                    var commit = new ProjectionCommit(projectionId, projectionType, version, @event, 1, eventOrigin, DateTime.UtcNow); // Snapshotting is disable till we test it => hardcoded 1
                    await projectionStore.SaveAsync(commit).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.ErrorException(ex, () => "Failed to persist event." + Environment.NewLine + $"\tProjectionVersion:{version}" + Environment.NewLine + $"\tEvent:{@event}");
                }
            }
        }

        public async Task<ReadResult<TState>> GetAsync<TState>(IBlobId projectionId) where TState : IProjectionDefinition
        {
            Type projectionType = typeof(TState);
            string projectionName = projectionType.GetContractId();

            using (logger.BeginScope(s => s
                       .AddScope(Log.ProjectionName, projectionType.GetContractId())
                       .AddScope(Log.ProjectionType, projectionType.Name)
                       .AddScope(Log.ProjectionInstanceId, projectionId.RawId)))
            {
                if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

                try
                {
                    (ProjectionStream, TState) streamState = ((ProjectionStream, TState))await LoadProjectionStreamAsync(projectionType, projectionId).ConfigureAwait(false);
                    TState projectionState = streamState.Item2 ?? await streamState.Item1.RestoreFromHistoryAsync<TState>().ConfigureAwait(false);

                    ReadResult<TState> readResult = new(projectionState);

                    if (readResult.NotFound)
                        logger.Debug(() => "Projection instance not found.");

                    return readResult;
                }
                catch (Exception ex)
                {
                    logger.ErrorException(ex, () => "Unable to load projection.");
                    return ReadResult<TState>.WithError(ex);
                }
            }
        }

        public async Task<ReadResult<IProjectionDefinition>> GetAsync(IBlobId projectionId, Type projectionType)
        {
            string projectionName = projectionType.GetContractId();

            using (logger.BeginScope(s => s
                       .AddScope(Log.ProjectionName, projectionType.GetContractId())
                       .AddScope(Log.ProjectionType, projectionType.Name)
                       .AddScope(Log.ProjectionInstanceId, projectionId.RawId)))
            {
                if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

                try
                {
                    (ProjectionStream, IProjectionDefinition) streamState = await LoadProjectionStreamAsync(projectionType, projectionId).ConfigureAwait(false);
                    IProjectionDefinition projectionState = streamState.Item2 ?? await streamState.Item1.RestoreFromHistoryAsync(projectionType).ConfigureAwait(false);

                    ReadResult<IProjectionDefinition> readResult = new ReadResult<IProjectionDefinition>(projectionState);

                    if (readResult.NotFound)
                        logger.Debug(() => "Projection instance not found.");

                    return readResult;
                }
                catch (Exception ex)
                {
                    logger.ErrorException(ex, () => "Unable to load projection.");
                    return ReadResult<IProjectionDefinition>.WithError(ex);
                }
            }
        }

        private async Task<(ProjectionStream, IProjectionDefinition)> LoadProjectionStreamAsync(Type projectionType, IBlobId projectionId)
        {
            string projectionName = projectionType.GetContractId();
            IProjectionDefinition projection = default;

            ReadResult<ProjectionVersions> result = await GetProjectionVersionsAsync(projectionName).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                ProjectionVersion liveVersion = result.Data.GetLive();
                if (liveVersion is null)
                {
                    logger.Warn(() => $"Unable to find projection `live` version for {projectionType.Name}. {Environment.NewLine}AvailableVersions:{Environment.NewLine}{result.Data}");
                    return (ProjectionStream.Empty(), projection);
                }

                ProjectionStream stream = await LoadProjectionStreamAsync(projectionType, liveVersion, projectionId);

                return (stream, projection);
            }

            return (ProjectionStream.Empty(), projection);
        }

        private async Task<ProjectionStream> LoadProjectionStreamAsync(Type projectionType, ProjectionVersion version, IBlobId projectionId)
        {
            string projectionName = projectionType.GetContractId();
            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();

            SnapshotId snapshotId = new SnapshotId(projectionName, context.Tenant);
            ISnapshot snapshot = await snapshotStore.LoadAsync(projectionName, snapshotId, version).ConfigureAwait(false);

            bool shouldLoadMore = true;
            while (shouldLoadMore)
            {
                var loadProjectionCommits = projectionStore.LoadAsync(version, projectionId, snapshot.Revision).ConfigureAwait(false);
                await foreach (var commit in loadProjectionCommits)
                    projectionCommits.Add(commit);

                shouldLoadMore = await projectionStore.HasSnapshotMarkerAsync(version, projectionId, snapshot.Revision + 1).ConfigureAwait(false);
            }
            ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, () => snapshot);
            return stream;
        }

        // Versioning related 
        protected async virtual Task<ReadResult<ProjectionVersions>> GetProjectionVersionsAsync(string projectionName)
        {
            if (string.IsNullOrEmpty(projectionName)) throw new ArgumentNullException(nameof(projectionName));

            var elapsed = new TimeSpan((long)(TimestampToTicks * (Stopwatch.GetTimestamp() - LastRefreshTimestamp)));

            ProjectionVersions versions = inMemoryVersionStore.Get(projectionName);

            //TODO: This optimization caused some problems
            //if (elapsed.TotalMinutes > 5 || versions is null || versions.Count == 0)
            {
                var queryResult = await GetProjectionVersionsFromStoreAsync(projectionName).ConfigureAwait(false);
                if (queryResult.IsSuccess)
                {
                    if (queryResult.Data.State.Live != null)
                        inMemoryVersionStore.Cache(queryResult.Data.State.Live);
                    foreach (var buildingVersion in queryResult.Data.State.AllVersions.GetBuildingVersions())
                    {
                        inMemoryVersionStore.Cache(buildingVersion);
                    }
                    versions = inMemoryVersionStore.Get(projectionName);
                    LastRefreshTimestamp = Stopwatch.GetTimestamp();
                }

                if (queryResult.HasError)
                    return ReadResult<ProjectionVersions>.WithError(queryResult.Error);
            }

            return new ReadResult<ProjectionVersions>(versions);
        }

        private async Task<ReadResult<ProjectionVersionsHandler>> GetProjectionVersionsFromStoreAsync(string projectionName)
        {
            try
            {
                var persistentVersionType = typeof(ProjectionVersionsHandler);
                var projectionVersions_ProjectionName = persistentVersionType.GetContractId();

                var versionId = new ProjectionVersionManagerId(projectionName, context.Tenant);
                var persistentVersion = new ProjectionVersion(projectionVersions_ProjectionName, ProjectionStatus.Live, 1, projectionHasher.CalculateHash(persistentVersionType));
                ProjectionStream stream = await LoadProjectionStreamAsync(persistentVersionType, persistentVersion, versionId, new NoSnapshot(versionId, projectionVersions_ProjectionName).GetMeta()).ConfigureAwait(false);
                var queryResult = await stream.RestoreFromHistoryAsync<ProjectionVersionsHandler>().ConfigureAwait(false);

                return new ReadResult<ProjectionVersionsHandler>(queryResult);
            }
            catch (Exception ex)
            {
                logger.WarnException(ex, () => $"Failed to load projection versions. ProjectionName: {projectionName}");
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
                //snapshotMarker++;
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

        private bool LogMessageWhenFailToUpdateProjection(Exception ex, ProjectionVersion version)
        {
            if (version.Status == ProjectionStatus.Live)
                logger.ErrorException(ex, () => $"Failed to update projection. Please replay the projection to restore the state.");
            else
                logger.WarnException(ex, () => $"Failed to update projection. Most probably the event will be replayed later so you could ignore this message OR log a bug.");

            return true;
        }

        private bool ShouldSaveEventForVersion(ProjectionVersion version)
        {
            return version.Status == ProjectionStatus.Building || version.Status == ProjectionStatus.Replaying || version.Status == ProjectionStatus.Rebuilding || version.Status == ProjectionStatus.Live;
        }

        //private async Task<ISnapshot> GetSnapshotAsync(Type projectionType, IBlobId projectionId, ProjectionVersion version)
        //{
        //    string projectionName = projectionType.GetContractId();

        //    if (projectionType.IsSnapshotable())
        //    {
        //        // Read snapshot projection
        //        ReadResult<SnapshotProjection> snapshotResult = await GetProjectionSnapshotsFromStoreAsync(projectionName).ConfigureAwait(false);

        //        if (snapshotResult.IsSuccess && snapshotResult.Data.State.Snapshot is not null)
        //            return snapshotResult.Data.State.Snapshot;
        //    }

        //    return new NoSnapshot(projectionId, projectionName);
        //}

        //private async Task<ReadResult<SnapshotProjection>> GetProjectionSnapshotsFromStoreAsync(string projectionName)
        //{
        //    try
        //    {
        //        Type snapshotType = typeof(SnapshotProjection);
        //        SnapshotId snapshotId = new SnapshotId(projectionName, context.Tenant);

        //        var stream = await LoadProjectionStreamAsync(snapshotType, snapshotId).ConfigureAwait(false);
        //        SnapshotProjection queryResult = await stream.Item1.RestoreFromHistoryAsync<SnapshotProjection>().ConfigureAwait(false);

        //        return new ReadResult<SnapshotProjection>(queryResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.WarnException(ex, () => $"Failed to load projection versions. ProjectionName: {projectionName}");
        //        return ReadResult<SnapshotProjection>.WithError(ex.Message);
        //    }
        //}
    }
}
