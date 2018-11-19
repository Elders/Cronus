using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Snapshotting;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.Projections
{
    public class ProjectionRepository : IProjectionWriter, IProjectionReader
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
        private static readonly ILog log = LogProvider.GetLogger(typeof(ProjectionRepository));
        private static long LastRefreshTimestamp = 0;

        private readonly CronusContext context;
        readonly IProjectionStore projectionStore;
        readonly ISnapshotStore snapshotStore;
        readonly ISnapshotStrategy snapshotStrategy;
        readonly InMemoryProjectionVersionStore inMemoryVersionStore;
        private readonly IHandlerFactory handlerFactory;

        public ProjectionRepository(CronusContext context, IProjectionStore projectionStore, ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy, InMemoryProjectionVersionStore inMemoryVersionStore, IHandlerFactory handlerFactory)
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
        }

        public void Initialize(ProjectionVersion version)
        {
            var initializableProjectionStore = projectionStore as IInitializableProjectionStore;
            initializableProjectionStore?.Initialize(version);
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
                    foreach (ProjectionVersion version in GetProjectionVersions(projectionName))
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
                                ProjectionStream projectionStream = LoadProjectionStream(projectionType, version, projectionId, snapshotMeta);
                                int snapshotMarker = snapshotStrategy.GetSnapshotMarker(projectionStream.Commits, snapshotMeta.Revision);

                                var commit = new ProjectionCommit(projectionId, version, @event, snapshotMarker, eventOrigin, DateTime.UtcNow);
                                projectionStore.Save(commit);
                            }
                            catch (Exception ex)
                            {
                                log.ErrorException("Failed to persist event." + Environment.NewLine + $"\tProjectionVersion:{version}" + Environment.NewLine + $"\tEvent:{@event}", ex);
                            }
                        }
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

            var projection = FastActivator.CreateInstance(projectionType) as IProjectionDefinition;
            if (projection != null)
            {
                var projectionIds = projection.GetProjectionIds(@event);

                foreach (var projectionId in projectionIds)
                {
                    try
                    {
                        SnapshotMeta snapshotMeta = null;
                        if (projectionType.IsSnapshotable())
                            snapshotMeta = snapshotStore.LoadMeta(projectionName, projectionId, version);
                        else
                            snapshotMeta = new NoSnapshot(projectionId, projectionName).GetMeta();
                        ProjectionStream projectionStream = LoadProjectionStream(projectionType, version, projectionId, snapshotMeta);
                        int snapshotMarker = snapshotStrategy.GetSnapshotMarker(projectionStream.Commits, snapshotMeta.Revision);

                        var commit = new ProjectionCommit(projectionId, version, @event, snapshotMarker, eventOrigin, DateTime.UtcNow);
                        projectionStore.Save(commit);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorException("Failed to persist event." + Environment.NewLine + $"\tProjectionVersion:{version}" + Environment.NewLine + $"\tEvent:{@event}", ex);
                    }
                }
            }
        }

        public ReadResult<T> Get<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

            Type projectionType = typeof(T);

            ProjectionStream stream = LoadProjectionStream(projectionType, projectionId);
            return new ReadResult<T>(stream.RestoreFromHistory<T>());
        }

        public ReadResult<IProjectionDefinition> Get(IBlobId projectionId, Type projectionType)
        {
            if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

            ProjectionStream stream = LoadProjectionStream(projectionType, projectionId);
            return new ReadResult<IProjectionDefinition>(stream.RestoreFromHistory(projectionType));
        }

        public async Task<ReadResult<T>> GetAsync<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

            Type projectionType = typeof(T);

            ProjectionStream stream = await LoadProjectionStreamAsync(projectionType, projectionId);
            return new ReadResult<T>(stream.RestoreFromHistory<T>());
        }

        public async Task<ReadResult<IProjectionDefinition>> GetAsync(IBlobId projectionId, Type projectionType)
        {
            if (ReferenceEquals(null, projectionId)) throw new ArgumentNullException(nameof(projectionId));

            ProjectionStream stream = await LoadProjectionStreamAsync(projectionType, projectionId);
            return new ReadResult<IProjectionDefinition>(stream.RestoreFromHistory(projectionType));
        }

        ProjectionVersions GetProjectionVersions(string projectionName)
        {
            if (string.IsNullOrEmpty(projectionName)) throw new ArgumentNullException(nameof(projectionName));

            try
            {
                var elapsed = new TimeSpan((long)(TimestampToTicks * (Stopwatch.GetTimestamp() - LastRefreshTimestamp)));

                ProjectionVersions versions = inMemoryVersionStore.Get(projectionName);
                if (versions is null || versions.Count == 0 || elapsed.TotalMinutes > 5)
                {
                    var queryResult = GetProjectionVersionsFromStore();
                    if (queryResult.IsSuccess)
                    {
                        if (queryResult.Data.State.Live != null)
                            inMemoryVersionStore.Cache(queryResult.Data.State.Live);
                        foreach (var buildingVersion in queryResult.Data.State.AllVersions.Where(x => x.Status == ProjectionStatus.Building))
                        {
                            inMemoryVersionStore.Cache(buildingVersion.WithStatus(ProjectionStatus.Live));
                        }
                        versions = inMemoryVersionStore.Get(projectionName);
                        LastRefreshTimestamp = Stopwatch.GetTimestamp();
                    }
                }

                return versions ?? new ProjectionVersions();

            }
            catch (Exception ex)
            {
                log.WarnException($"Unable to load projection versions. ProjectionName:{projectionName}", ex);
                return new ProjectionVersions();
            }
        }

        ReadResult<ProjectionVersionsHandler> GetProjectionVersionsFromStore()
        {
            var persistentVersionType = typeof(ProjectionVersionsHandler);
            var projectionVersions_ProjectionName = persistentVersionType.GetContractId();

            var versionId = new ProjectionVersionManagerId(projectionVersions_ProjectionName, context.Tenant);
            var persistentVersion = new ProjectionVersion(projectionVersions_ProjectionName, ProjectionStatus.Live, 1, persistentVersionType.GetProjectionHash());
            ProjectionStream stream = LoadProjectionStream(persistentVersionType, persistentVersion, versionId, new NoSnapshot(versionId, projectionVersions_ProjectionName).GetMeta());
            var queryResult = stream.RestoreFromHistory<ProjectionVersionsHandler>();

            return new ReadResult<ProjectionVersionsHandler>(queryResult);
        }

        ProjectionStream LoadProjectionStream(Type projectionType, IBlobId projectionId)
        {
            string projectionName = projectionType.GetContractId();

            try
            {
                ProjectionVersion liveVersion = GetProjectionVersions(projectionName).GetLive();
                if (liveVersion is null)
                {
                    log.Warn(() => $"Unable to find projection `live` version. ProjectionId:{projectionId} ProjectionName:{projectionName} ProjectionType:{projectionType.Name}");
                    return ProjectionStream.Empty();
                }

                ISnapshot snapshot = null;
                if (projectionType.IsSnapshotable())
                    snapshot = snapshotStore.Load(projectionName, projectionId, liveVersion);
                else
                    snapshot = new NoSnapshot(projectionId, projectionName);

                ProjectionStream stream = LoadProjectionStream(projectionType, liveVersion, projectionId, snapshot);

                return stream;
            }
            catch (Exception ex)
            {
                log.ErrorException($"Unable to load projection stream. ProjectionId:{projectionId} ProjectionName:{projectionName} ProjectionType:{projectionType.Name}", ex);
                return ProjectionStream.Empty();
            }
        }

        ProjectionStream LoadProjectionStream(Type projectionType, ProjectionVersion version, IBlobId projectionId, ISnapshot snapshot)
        {
            Func<ISnapshot> loadSnapshot = () => snapshot;

            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();
            int snapshotMarker = snapshot.Revision;
            while (true)
            {
                snapshotMarker++;
                var loadedCommits = projectionStore.Load(version, projectionId, snapshotMarker).ToList();
                projectionCommits.AddRange(loadedCommits);

                if (projectionType.IsSnapshotable() && snapshotStrategy.ShouldCreateSnapshot(projectionCommits, snapshot.Revision))
                {
                    ProjectionStream checkpointStream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
                    var projectionState = checkpointStream.RestoreFromHistory(projectionType).State;
                    ISnapshot newSnapshot = new Snapshot(projectionId, version.ProjectionName, projectionState, snapshot.Revision + 1);
                    snapshotStore.Save(newSnapshot, version);
                    loadSnapshot = () => newSnapshot;

                    projectionCommits.Clear();

                    log.Debug(() => $"Snapshot created for projection `{version.ProjectionName}` with id={projectionId} where ({loadedCommits.Count}) were zipped. Snapshot: `{snapshot.GetType().Name}`");
                }

                if (loadedCommits.Count < snapshotStrategy.EventsInSnapshot)
                    break;
                else
                    log.Warn(() => $"Potential memory leak. The system will be down fairly soon. The projection `{version.ProjectionName}` with id={projectionId} loads a lot of projection commits ({loadedCommits.Count}) and snapshot `{snapshot.GetType().Name}` which puts a lot of CPU and RAM pressure. You can resolve this by configuring the snapshot settings`.");
            }

            ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
            return stream;
        }

        ProjectionStream LoadProjectionStream(Type projectionType, ProjectionVersion version, IBlobId projectionId, SnapshotMeta snapshotMeta)
        {
            Func<ISnapshot> loadSnapshot = () => snapshotStore.Load(version.ProjectionName, projectionId, version);

            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();
            int snapshotMarker = snapshotMeta.Revision;
            while (true)
            {
                snapshotMarker++;
                var loadedCommits = projectionStore.Load(version, projectionId, snapshotMarker).ToList();
                projectionCommits.AddRange(loadedCommits);

                if (projectionType.IsSnapshotable())
                {
                    if (snapshotStrategy.ShouldCreateSnapshot(projectionCommits, snapshotMeta.Revision))
                    {
                        ProjectionStream checkpointStream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
                        var projectionState = checkpointStream.RestoreFromHistory(projectionType).State;
                        ISnapshot newSnapshot = new Snapshot(projectionId, version.ProjectionName, projectionState, snapshotMeta.Revision + 1);
                        snapshotStore.Save(newSnapshot, version);
                        loadSnapshot = () => newSnapshot;

                        projectionCommits.Clear();

                        log.Debug(() => $"Snapshot created for projection `{version.ProjectionName}` with id={projectionId} where ({loadedCommits.Count}) were zipped. Snapshot: `{newSnapshot.GetType().Name}`");
                    }
                }
                else
                    loadSnapshot = () => new NoSnapshot(projectionId, version.ProjectionName);

                if (loadedCommits.Count < snapshotStrategy.EventsInSnapshot)
                    break;
                else
                    log.Warn(() => $"Potential memory leak. The system will be down fairly soon. The projection `{version.ProjectionName}` with id={projectionId} loads a lot of projection commits ({loadedCommits.Count}) which puts a lot of CPU and RAM pressure. You can resolve this by configuring the snapshot settings`.");
            }

            ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
            return stream;
        }

        async Task<ProjectionStream> LoadProjectionStreamAsync(Type projectionType, IBlobId projectionId)
        {
            string projectionName = projectionType.GetContractId();

            try
            {
                ProjectionVersion liveVersion = GetProjectionVersions(projectionName).GetLive();
                if (liveVersion is null)
                {
                    log.Warn(() => $"Unable to find projection `live` version. ProjectionId:{projectionId} ProjectionName:{projectionName} ProjectionType:{projectionType.Name}");
                    return ProjectionStream.Empty();
                }

                ISnapshot snapshot = null;
                if (projectionType.IsSnapshotable())
                    snapshot = snapshotStore.Load(projectionName, projectionId, liveVersion);
                else
                    snapshot = new NoSnapshot(projectionId, projectionName);

                ProjectionStream stream = await LoadProjectionStreamAsync(projectionType, liveVersion, projectionId, snapshot);

                return stream;
            }
            catch (Exception ex)
            {
                log.ErrorException($"Unable to load projection stream. ProjectionId:{projectionId} ProjectionName:{projectionName} ProjectionType:{projectionType.Name}", ex);
                return ProjectionStream.Empty();
            }
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

                if (projectionType.IsSnapshotable() && snapshotStrategy.ShouldCreateSnapshot(projectionCommits, snapshot.Revision))
                {
                    ProjectionStream checkpointStream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
                    var projectionState = checkpointStream.RestoreFromHistory(projectionType).State;
                    ISnapshot newSnapshot = new Snapshot(projectionId, version.ProjectionName, projectionState, snapshot.Revision + 1);
                    snapshotStore.Save(newSnapshot, version);
                    loadSnapshot = () => newSnapshot;

                    projectionCommits.Clear();

                    log.Debug(() => $"Snapshot created for projection `{version.ProjectionName}` with id={projectionId} where ({loadedCommits.Count}) were zipped. Snapshot: `{snapshot.GetType().Name}`");
                }

                if (loadedCommits.Count < snapshotStrategy.EventsInSnapshot)
                    break;
                else
                    log.Warn(() => $"Potential memory leak. The system will be down fairly soon. The projection `{version.ProjectionName}` with id={projectionId} loads a lot of projection commits ({loadedCommits.Count}) and snapshot `{snapshot.GetType().Name}` which puts a lot of CPU and RAM pressure. You can resolve this by configuring the snapshot settings`.");
            }

            ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, loadSnapshot);
            return stream;
        }

    }
}
