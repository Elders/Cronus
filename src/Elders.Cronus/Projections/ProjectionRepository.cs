using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Logging;
using Elders.Cronus.Projections.Snapshotting;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.Projections.Cassandra.EventSourcing
{
    public class ProjectionRepository : IProjectionRepository
    {
        static ILog log = LogProvider.GetLogger(typeof(ProjectionRepository));

        readonly IProjectionStore projectionStore;
        readonly ISnapshotStore snapshotStore;
        readonly ISnapshotStrategy snapshotStrategy;
        readonly InMemoryProjectionVersionStore inMemoryVersionStore;

        public ProjectionRepository(IProjectionStore projectionStore, ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy, InMemoryProjectionVersionStore inMemoryVersionStore)
        {
            if (ReferenceEquals(null, projectionStore) == true) throw new ArgumentException(nameof(projectionStore));
            if (ReferenceEquals(null, snapshotStore) == true) throw new ArgumentException(nameof(snapshotStore));
            if (ReferenceEquals(null, snapshotStrategy) == true) throw new ArgumentException(nameof(snapshotStrategy));
            if (ReferenceEquals(null, inMemoryVersionStore) == true) throw new ArgumentException(nameof(inMemoryVersionStore));

            this.projectionStore = projectionStore;
            this.snapshotStore = snapshotStore;
            this.snapshotStrategy = snapshotStrategy;
            this.inMemoryVersionStore = inMemoryVersionStore;
        }

        ProjectionStream LoadProjectionStream(Type projectionType, ProjectionVersion projectionVersion, IBlobId projectionId, ISnapshot snapshot)
        {
            ISnapshot currentSnapshot = snapshot;
            string contractId = projectionVersion.ProjectionContractId;

            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();
            int snapshotMarker = snapshot.Revision;
            while (true)
            {
                snapshotMarker++;
                var loadedCommits = projectionStore.Load(projectionVersion, projectionId, snapshotMarker).ToList();
                projectionCommits.AddRange(loadedCommits);
                bool isSnapshotable = typeof(IAmNotSnapshotable).IsAssignableFrom(projectionType) == false;

                if (isSnapshotable && snapshotStrategy.ShouldCreateSnapshot(projectionCommits, snapshot.Revision))
                {
                    ProjectionStream checkpointStream = new ProjectionStream(projectionId, projectionCommits, currentSnapshot);
                    var projectionState = checkpointStream.RestoreFromHistory(projectionType).Projection.State;
                    ISnapshot newSnapshot = new Snapshot(projectionId, contractId, projectionState, snapshot.Revision + 1);
                    snapshotStore.Save(newSnapshot, projectionVersion);
                    currentSnapshot = newSnapshot;

                    projectionCommits.Clear();

                    log.Debug(() => $"Snapshot created for projection `{contractId}` with id={projectionId} where ({loadedCommits.Count}) were zipped. Snapshot: `{snapshot.GetType().Name}`");
                }
                else if (loadedCommits.Count() < snapshotStrategy.EventsInSnapshot)
                {
                    break;
                }
                else
                {
                    log.Warn($"Potential memory leak. The system will be down fairly soon. The projection `{contractId}` with id={projectionId} loads a lot of projection commits ({loadedCommits.Count}) and snapshot `{snapshot.GetType().Name}` which puts a lot of CPU and RAM pressure. You can resolve this by configuring the snapshot settings`.");
                }
            }

            ProjectionStream stream = new ProjectionStream(projectionId, projectionCommits, currentSnapshot);
            return stream;
        }

        public IProjectionGetResult<T> Get<T>(IBlobId projectionId) where T : IProjectionDefinition
        {
            try
            {
                Type projectionType = typeof(T);
                string contractId = projectionType.GetContractId();

                ProjectionVersion liveVersion = GetProjectionVersions(contractId).GetLive();

                ISnapshot snapshot = snapshotStore.Load(contractId, projectionId, liveVersion);
                ProjectionStream stream = LoadProjectionStream(projectionType, liveVersion, projectionId, snapshot);
                IProjectionGetResult<T> queryResult = stream.RestoreFromHistory<T>();

                return queryResult;
            }
            catch (Exception ex)
            {
                log.ErrorException(ex.Message, ex);
                return new ProjectionGetResult<T>(default(T));
            }
        }

        public IProjectionGetResult<IProjectionDefinition> Get(IBlobId projectionId, Type projectionType)
        {
            try
            {
                string contractId = projectionType.GetContractId();

                ProjectionVersion liveVersion = GetProjectionVersions(contractId).GetLive();

                ISnapshot snapshot = snapshotStore.Load(contractId, projectionId, liveVersion);
                ProjectionStream stream = LoadProjectionStream(projectionType, liveVersion, projectionId, snapshot);
                IProjectionGetResult<IProjectionDefinition> queryResult = stream.RestoreFromHistory(projectionType);

                return queryResult;
            }
            catch (Exception ex)
            {
                log.ErrorException(ex.Message, ex);
                return new ProjectionGetResult<IProjectionDefinition>(null);
            }
        }

        ProjectionVersions GetProjectionVersions(string contractId)
        {
            ProjectionVersions versions = inMemoryVersionStore.Get(contractId);
            if (versions == null || versions.Count == 0)
            {
                var versionId = new ProjectionVersionManagerId(contractId);
                var persistentVersionType = typeof(PersistentProjectionVersionHandler);
                var persistentVersionContractId = persistentVersionType.GetContractId();
                var persistentVersion = new ProjectionVersion(persistentVersionContractId, ProjectionStatus.Live, 1, persistentVersionType.GetProjectionHash());
                ProjectionStream stream = LoadProjectionStream(typeof(PersistentProjectionVersionHandler), persistentVersion, versionId, new NoSnapshot(versionId, contractId));
                var queryResult = stream.RestoreFromHistory<PersistentProjectionVersionHandler>();
                if (queryResult.Success)
                {
                    if (queryResult.Projection.State.Live != null)
                        inMemoryVersionStore.Cache(queryResult.Projection.State.Live);
                    if (queryResult.Projection.State.Building != null)
                        inMemoryVersionStore.Cache(queryResult.Projection.State.Building);
                    versions = inMemoryVersionStore.Get(contractId);
                }

                // inception
                if (versions == null || versions.Count == 0)
                {
                    if (string.Equals(persistentVersionContractId, contractId, StringComparison.OrdinalIgnoreCase))
                    {
                        inMemoryVersionStore.Cache(persistentVersion);
                        versions = inMemoryVersionStore.Get(contractId);
                    }
                    else
                    {
                        var initialVersion = new ProjectionVersion(contractId, ProjectionStatus.Building, 1, contractId.GetTypeByContract().GetProjectionHash());
                        inMemoryVersionStore.Cache(initialVersion);
                        versions = inMemoryVersionStore.Get(contractId);
                    }
                }
            }

            return versions ?? new ProjectionVersions();
        }

        public void Save(Type projectionType, IEvent @event, EventOrigin eventOrigin)
        {
            string contractId = projectionType.GetContractId();
            var instance = FastActivator.CreateInstance(projectionType);

            var statefullProjection = instance as IProjectionDefinition;
            if (statefullProjection != null)
            {
                var projectionIds = statefullProjection.GetProjectionIds(@event);

                foreach (var version in GetProjectionVersions(contractId))
                {
                    foreach (var projectionId in projectionIds)
                    {
                        ISnapshot snapshot = snapshotStore.Load(contractId, projectionId, version);
                        ProjectionStream projectionStream = LoadProjectionStream(projectionType, version, projectionId, snapshot);
                        int snapshotMarker = snapshotStrategy.GetSnapshotMarker(projectionStream.Commits, snapshot.Revision);

                        var commit = new ProjectionCommit(projectionId, version, @event, snapshotMarker, eventOrigin, DateTime.UtcNow);
                        projectionStore.Save(commit);
                    }
                }
            }
        }

        public void Save(Type projectionType, CronusMessage cronusMessage)
        {
            var projection = FastActivator.CreateInstance(projectionType) as IProjectionDefinition;
            if (projection != null)
            {
                var projectionIds = projection.GetProjectionIds(cronusMessage.Payload as IEvent);
                string contractId = projectionType.GetContractId();

                foreach (var projectionId in projectionIds)
                {
                    foreach (var version in GetProjectionVersions(contractId))
                    {
                        ISnapshot snapshot = snapshotStore.Load(contractId, projectionId, version);
                        ProjectionStream projectionStream = LoadProjectionStream(projectionType, version, projectionId, snapshot);
                        int snapshotMarker = snapshotStrategy.GetSnapshotMarker(projectionStream.Commits, snapshot.Revision);

                        EventOrigin eventOrigin = cronusMessage.GetEventOrigin();
                        DateTime timestamp = DateTime.UtcNow;
                        IEvent @event = cronusMessage.Payload as IEvent;

                        var commit = new ProjectionCommit(projectionId, version, @event, snapshotMarker, eventOrigin, timestamp);
                        projectionStore.Save(commit);
                    }
                }
            }
        }
    }
}
