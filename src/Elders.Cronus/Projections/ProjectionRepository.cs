using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Snapshotting;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Elders.Cronus.Projections
{
    public partial class ProjectionRepository : IProjectionWriter, IProjectionReader
    {
        private static readonly ILogger log = CronusLogger.CreateLogger(typeof(ProjectionRepository));
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
        private static long LastRefreshTimestamp = 0;

        private readonly CronusContext context;
        readonly IProjectionStore projectionStore;
        readonly ISnapshotStore snapshotStore;
        readonly ISnapshotStrategy snapshotStrategy;
        readonly InMemoryProjectionVersionStore inMemoryVersionStore;
        private readonly IHandlerFactory handlerFactory;
        private readonly ProjectionHasher projectionHasher;

        public ProjectionRepository(CronusContext context, IProjectionStore projectionStore, ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy, InMemoryProjectionVersionStore inMemoryVersionStore, IHandlerFactory handlerFactory, ProjectionHasher projectionHasher)
        {
            if (context is null) throw new ArgumentException(nameof(context));
            if (projectionStore is null) throw new ArgumentException(nameof(projectionStore));
            if (snapshotStore is null) throw new ArgumentException(nameof(snapshotStore));
            if (snapshotStrategy is null) throw new ArgumentException(nameof(snapshotStrategy));
            if (inMemoryVersionStore is null) throw new ArgumentException(nameof(inMemoryVersionStore));

            this.context = context;
            this.projectionStore = projectionStore;
            this.snapshotStore = snapshotStore;
            this.snapshotStrategy = snapshotStrategy;
            this.inMemoryVersionStore = inMemoryVersionStore;
            this.handlerFactory = handlerFactory;
            this.projectionHasher = projectionHasher;
        }

        public void Save(Type projectionType, CronusMessage cronusMessage)
        {
            if (ReferenceEquals(null, projectionType)) throw new ArgumentNullException(nameof(projectionType));
            if (ReferenceEquals(null, cronusMessage)) throw new ArgumentNullException(nameof(cronusMessage));

            EventOrigin eventOrigin = cronusMessage.GetEventOrigin();
            IEvent @event = cronusMessage.Payload as IEvent;

            Save(projectionType, @event, eventOrigin);
        }

        public void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin)
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
                    ReadResult<ProjectionVersions> result = GetProjectionVersions(projectionName);
                    if (result.IsSuccess)
                    {
                        foreach (ProjectionVersion version in result.Data)
                        {
                            if (version.Status == ProjectionStatus.Building || version.Status == ProjectionStatus.Live)
                            {
                                try
                                {
                                    SnapshotMeta snapshotMeta = null;
                                    if (projectionType.IsSnapshotable())
                                        snapshotMeta = snapshotStore.LoadMeta(projectionName, projectionId, version);
                                    else
                                        snapshotMeta = new NoSnapshot(projectionId, projectionName).GetMeta();

                                    int snapshotMarker = snapshotMeta.Revision + 2;

                                    var commit = new ProjectionCommit(projectionId, version, @event, snapshotMarker, eventOrigin, DateTime.UtcNow);
                                    projectionStore.Save(commit);
                                }
                                catch (Exception ex)
                                {
                                    log.ErrorException(ex, () => "Failed to update projection. Please replay the projection to restore the state. Self-heal hint!" + Environment.NewLine + $"\tProjectionVersion:{version}" + Environment.NewLine + $"\tEvent:{@event}");
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
        }

        public void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin, ProjectionVersion version)
        {
            if (ReferenceEquals(null, projectionType)) throw new ArgumentNullException(nameof(projectionType));
            if (ReferenceEquals(null, @event)) throw new ArgumentNullException(nameof(@event));
            if (ReferenceEquals(null, eventOrigin)) throw new ArgumentNullException(nameof(eventOrigin));
            if (ReferenceEquals(null, version)) throw new ArgumentNullException(nameof(version));

            if ((version.Status == ProjectionStatus.Building || version.Status == ProjectionStatus.Live) == false)
                throw new ArgumentException("Invalid version. Only versions in `Building` and `Live` status are eligable for persistence.", nameof(version));

            string projectionName = projectionType.GetContractId();
            if (projectionName.Equals(version.ProjectionName, StringComparison.OrdinalIgnoreCase) == false)
                throw new ArgumentException($"Invalid version. The version `{version}` does not match projection `{projectionName}`", nameof(version));

            var handlerInstance = handlerFactory.Create(projectionType);
            var projection = handlerInstance.Current as IProjectionDefinition;
            if (projection != null)
            {
                var projectionIds = projection.GetProjectionIds(@event);

                foreach (var projectionId in projectionIds)
                {
                    try
                    {
                        SnapshotMeta snapshotMeta = GetSnapshotMeta(projectionType, projectionName, projectionId, version);
                        int snapshotMarker = snapshotMeta.Revision == 0 ? 1 : snapshotMeta.Revision + 2;

                        var commit = new ProjectionCommit(projectionId, version, @event, snapshotMarker, eventOrigin, DateTime.UtcNow);
                        projectionStore.Save(commit);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorException(ex, () => "Failed to persist event." + Environment.NewLine + $"\tProjectionVersion:{version}" + Environment.NewLine + $"\tEvent:{@event}");
                    }
                }
            }
            else if (handlerInstance.Current is IAmEventSourcedProjection eventSourcedProjection)
            {
                try
                {
                    var projectionId = Urn.Parse($"urn:cronus:{projectionName}");

                    var commit = new ProjectionCommit(projectionId, version, @event, 1, eventOrigin, DateTime.UtcNow);
                    projectionStore.Save(commit);
                }
                catch (Exception ex)
                {
                    log.ErrorException(ex, () => "Failed to persist event." + Environment.NewLine + $"\tProjectionVersion:{version}" + Environment.NewLine + $"\tEvent:{@event}");
                }
            }
        }

        private SnapshotMeta GetSnapshotMeta(Type projectionType, string projectionName, IBlobId projectionId, ProjectionVersion version)
        {
            SnapshotMeta snapshotMeta;
            if (projectionType.IsSnapshotable())
                snapshotMeta = snapshotStore.LoadMeta(projectionName, projectionId, version);
            else
                snapshotMeta = new NoSnapshot(projectionId, projectionName).GetMeta();

            return snapshotMeta;
        }

        public ReadResult<T> Get<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            try
            {
                if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

                Type projectionType = typeof(T);

                ProjectionStream stream = LoadProjectionStream(projectionType, projectionId);
                return new ReadResult<T>(stream.RestoreFromHistory<T>());
            }
            catch (Exception ex)
            {
                log.ErrorException(ex, () => $"Unable to load projection. {typeof(T).Name}({projectionId})");
                return ReadResult<T>.WithError(ex);
            }
        }

        public ReadResult<IProjectionDefinition> Get(IBlobId projectionId, Type projectionType)
        {
            if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

            try
            {
                ProjectionStream stream = LoadProjectionStream(projectionType, projectionId);
                return new ReadResult<IProjectionDefinition>(stream.RestoreFromHistory(projectionType));
            }
            catch (Exception ex)
            {
                log.ErrorException(ex, () => $"Unable to load projection. {projectionType.Name}({projectionId})");
                return ReadResult<IProjectionDefinition>.WithError(ex);
            }
        }

        protected virtual ReadResult<ProjectionVersions> GetProjectionVersions(string projectionName)
        {
            if (string.IsNullOrEmpty(projectionName)) throw new ArgumentNullException(nameof(projectionName));

            var elapsed = new TimeSpan((long)(TimestampToTicks * (Stopwatch.GetTimestamp() - LastRefreshTimestamp)));

            ProjectionVersions versions = inMemoryVersionStore.Get(projectionName);

            //TODO: This optimization caused some problems
            //if (elapsed.TotalMinutes > 5 || versions is null || versions.Count == 0)
            {
                var queryResult = GetProjectionVersionsFromStore(projectionName);
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

        ReadResult<ProjectionVersionsHandler> GetProjectionVersionsFromStore(string projectionName)
        {
            try
            {
                var persistentVersionType = typeof(ProjectionVersionsHandler);
                var projectionVersions_ProjectionName = persistentVersionType.GetContractId();

                var versionId = new ProjectionVersionManagerId(projectionName, context.Tenant);
                var persistentVersion = new ProjectionVersion(projectionVersions_ProjectionName, ProjectionStatus.Live, 1, projectionHasher.CalculateHash(persistentVersionType));
                ProjectionStream stream = LoadProjectionStream(persistentVersionType, persistentVersion, versionId, new NoSnapshot(versionId, projectionVersions_ProjectionName).GetMeta());
                var queryResult = stream.RestoreFromHistory<ProjectionVersionsHandler>();

                return new ReadResult<ProjectionVersionsHandler>(queryResult);
            }
            catch (Exception ex)
            {
                log.WarnException(ex, () => $"Failed to load projection versions. ProjectionName: {projectionName}");
                return ReadResult<ProjectionVersionsHandler>.WithError(ex.Message);
            }
        }

        ProjectionStream LoadProjectionStream(Type projectionType, IBlobId projectionId)
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
                    ? snapshotStore.Load(projectionName, projectionId, liveVersion)
                    : new NoSnapshot(projectionId, projectionName);

                return LoadProjectionStream(projectionType, liveVersion, projectionId, SnapshotMeta.From(snapshot), () => snapshot);
            }

            return ProjectionStream.Empty();
        }

        ProjectionStream LoadProjectionStream(Type projectionType, ProjectionVersion version, IBlobId projectionId, SnapshotMeta snapshotMeta)
        {
            Func<ISnapshot> loadSnapshot = () => projectionType.IsSnapshotable()
                ? snapshotStore.Load(version.ProjectionName, projectionId, version)
                : new NoSnapshot(projectionId, version.ProjectionName);

            return LoadProjectionStream(projectionType, version, projectionId, snapshotMeta, loadSnapshot);
        }

        ProjectionStream LoadProjectionStream(Type projectionType, ProjectionVersion version, IBlobId projectionId, SnapshotMeta snapshotMeta, Func<ISnapshot> loadSnapshot)
        {
            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();
            int snapshotMarker = snapshotMeta.Revision;

            bool shouldLoadMore = true;
            while (shouldLoadMore)
            {
                snapshotMarker++;
                var loadedCommits = projectionStore.Load(version, projectionId, snapshotMarker);
                projectionCommits.AddRange(loadedCommits);

                shouldLoadMore = projectionStore.HasSnapshotMarker(version, projectionId, snapshotMarker + 1);
            }

            ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
            return stream;
        }
    }
}
