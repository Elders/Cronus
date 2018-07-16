using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Elders.Cronus.Projections.Snapshotting;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.Projections
{
    public sealed class ReplayResult
    {
        public ReplayResult(string error = null, bool isTimeOut = false)
        {
            Error = error;
            IsTimeout = isTimeOut;
        }

        public string Error { get; private set; }

        public bool IsSuccess { get { return string.IsNullOrEmpty(Error) && IsTimeout != true; } }

        public bool IsTimeout { get; private set; }
    }

    public interface IProjectionPlayer { }

    public class ProjectionPlayer : IProjectionPlayer
    {
        static ILog log = LogProvider.GetLogger(typeof(ProjectionPlayer));

        object playerSync = new object();
        private bool isBuilding = false;
        private readonly IEventStoreFactory eventStoreFactory;
        private readonly IProjectionStore projectionStore;
        private readonly IProjectionRepository projectionRepository;
        private readonly ISnapshotStore snapshotStore;
        private readonly EventTypeIndexForProjections index;
        private readonly ITenantResolver tenantResolver;

        public ProjectionPlayer(IEventStoreFactory eventStoreFactory, IProjectionStore projectionStore, IProjectionRepository projectionRepository, ISnapshotStore snapshotStore, EventTypeIndexForProjections index, ITenantResolver tenantResolver)
        {
            this.eventStoreFactory = eventStoreFactory;
            this.projectionStore = projectionStore;
            this.projectionRepository = projectionRepository;
            this.snapshotStore = snapshotStore;
            this.index = index;
            this.tenantResolver = tenantResolver;
        }

        public ReplayResult Rebuild(Type projectionType, ProjectionVersion version, DateTime replayUntil)
        {
            if (ReferenceEquals(null, version)) throw new ArgumentNullException(nameof(version));

            try
            {
                if (IsVersionOutdated(version))
                {
                    return new ReplayResult($"Version `{version}` is outdated. There is a newer one which is already live.");
                }

                if (IsCanceled(version))
                {
                    return new ReplayResult($"Version `{version}` was canceled.");
                }
                DateTime startRebuildTimestamp = DateTime.UtcNow;
                int progressCounter = 0;
                log.Info(() => $"Start rebuilding projection `{projectionType.Name}` for version {version}. Deadline is {replayUntil}");

                var projection = FastActivator.CreateInstance(projectionType) as IProjectionDefinition;
                var projectionEventTypes = GetInvolvedEvents(projectionType).ToList();

                projectionStore.InitializeProjectionStore(version);
                snapshotStore.InitializeProjectionSnapshotStore(version);
                var indexState = index.GetIndexState();
                if (indexState.IsPresent() == false)
                    return new ReplayResult("Projection index does not exists");
                foreach (var eventType in projectionEventTypes)
                {
                    Dictionary<int, string> processedAggregates = new Dictionary<int, string>();

                    log.Debug(() => $"Rebuilding projection `{projectionType.Name}` for version {version} using eventType `{eventType}`. Deadline is {replayUntil}");

                    var indexId = new EventStoreIndexEventTypeId(eventType);
                    IEnumerable<ProjectionCommit> indexCommits = index.EnumerateCommitsByEventType(indexId);

                    foreach (var indexCommit in indexCommits)
                    {
                        progressCounter++;
                        if (progressCounter % 1000 == 0)
                        {
                            log.Trace(() => $"Rebuilding projection {projectionType.Name} => PROGRESS:{progressCounter} Version:{version} EventType:{eventType} Deadline:{replayUntil} Total minutes working:{(DateTime.UtcNow - startRebuildTimestamp).TotalMinutes}. logId:{Guid.NewGuid().ToString()} ProcessedAggregatesSize:{processedAggregates.Count}");
                        }

                        if (DateTime.UtcNow >= replayUntil)
                        {
                            string message = $"Rebuilding projection `{projectionType.Name}` takes longer than expected. PROGRESS:{progressCounter} Version:{version} EventType:`{eventType}` Deadline:{replayUntil}.";
                            return new ReplayResult(message, true);
                        }

                        if (processedAggregates.ContainsKey(indexCommit.EventOrigin.AggregateRootId.GetHashCode()))
                            continue;

                        processedAggregates.Add(indexCommit.EventOrigin.AggregateRootId.GetHashCode(), null);

                        IAggregateRootId arId = GetAggregateRootId(indexCommit.EventOrigin.AggregateRootId);
                        IEventStore eventStore = eventStoreFactory.GetEventStore(tenantResolver.Resolve(arId));
                        EventStream stream = eventStore.Load(arId);

                        foreach (AggregateCommit arCommit in stream.Commits)
                        {
                            for (int i = 0; i < arCommit.Events.Count; i++)
                            {
                                IEvent theEvent = arCommit.Events[i].Unwrap();

                                if (projectionEventTypes.Contains(theEvent.GetType().GetContractId()))
                                {
                                    var origin = new EventOrigin(Convert.ToBase64String(arCommit.AggregateRootId), arCommit.Revision, i, arCommit.Timestamp);
                                    projectionRepository.Save(projectionType, theEvent, origin, version);
                                }
                            }
                        }
                    }
                }
                log.Info(() => $"Finish rebuilding projection `{projectionType.Name}` for version {version}. Deadline was {replayUntil}");
                return new ReplayResult();
            }
            catch (Exception ex)
            {
                string message = $"Unable to replay projection. Version:{version} ProjectionType:{projectionType.FullName}";
                log.ErrorException(message, ex);
                return new ReplayResult(message + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public bool HasIndex()
        {
            log.Info(() => "Getting index state");
            var indexState = index.GetIndexState();
            var indexIsPresent = indexState.IsPresent();

            if (indexIsPresent)
                log.Info(() => "Index is present");
            else
                log.Info(() => "Index is not present");

            return indexIsPresent;
        }

        public bool RebuildIndex()
        {
            var indexBuilder = index.GetIndexBuilder();
            isBuilding = index.GetIndexState().IsBuilding();

            if (isBuilding == false)
            {
                lock (playerSync)
                {
                    if (isBuilding == false)
                    {
                        indexBuilder.Prepare();
                        isBuilding = true;
                    }
                    else
                    {
                        log.Debug(() => "Index is currently built by someone");
                        return false;
                    }
                }
            }
            else
            {
                log.Debug(() => "Index is currently built by someone");
                return false;
            }

            try
            {
                log.Info(() => "Start rebuilding index...");

                var eventStorePlayers = eventStoreFactory.GetEventStorePlayers();

                var eventsCounter = 0;
                foreach (var eventStorePlayer in eventStorePlayers)
                {
                    foreach (var aggregateCommit in eventStorePlayer.LoadAggregateCommits())
                    {
                        foreach (var @event in aggregateCommit.Events)
                        {
                            try
                            {
                                if (eventsCounter % 1000 == 0)
                                    log.Info(() => $"Rebuilding index progress: {eventsCounter}");

                                var unwrapedEvent = @event.Unwrap();
                                var rootId = System.Text.Encoding.UTF8.GetString(aggregateCommit.AggregateRootId);
                                var eventOrigin = new EventOrigin(rootId, aggregateCommit.Revision, aggregateCommit.Events.IndexOf(@event), aggregateCommit.Timestamp);
                                indexBuilder.Feed(unwrapedEvent, eventOrigin);
                            }
                            catch (Exception ex)
                            {
                                log.ErrorException($"Rebuilding index for event {@event.ToString()} failed with {ex.Message}", ex);
                            }

                            eventsCounter++;
                        }
                    }
                }

                indexBuilder.Complete();

                log.Info(() => "Completed rebuilding index");
                isBuilding = false;

                return true;
            }
            catch (Exception ex)
            {
                log.ErrorException("Failed to rebuild index", ex);
                return false;
            }
        }

        IEnumerable<string> GetInvolvedEvents(Type projectionType)
        {
            var ieventHandler = typeof(IEventHandler<>);
            var interfaces = projectionType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().First();
                yield return eventType.GetContractId();
            }
        }

        IAggregateRootId GetAggregateRootId(string mess)
        {
            var parts = mess.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                StringTenantUrn urn;
                if (StringTenantUrn.TryParse(part, out urn))
                {
                    return new StringTenantId(urn, urn.ArName);
                }
                else
                {
                    byte[] raw = Convert.FromBase64String(part);
                    string urnString = Encoding.UTF8.GetString(raw);
                    if (StringTenantUrn.TryParse(urnString, out urn))
                    {
                        return new StringTenantId(urn, urn.ArName);
                    }
                }
            }

            throw new ArgumentException($"Invalid aggregate root id: {mess}", nameof(mess));
        }

        ProjectionVersions GetProjectionVersionsFromStore(ProjectionVersion version)
        {
            var versionId = new ProjectionVersionManagerId(version.ProjectionName);

            var persistentVersionType = typeof(ProjectionVersionsHandler);
            var projectionName = persistentVersionType.GetContractId();

            var persistentVersion = new ProjectionVersion(projectionName, ProjectionStatus.Live, 1, persistentVersionType.GetProjectionHash());


            List<ProjectionCommit> projectionCommits = new List<ProjectionCommit>();

            var loadedCommits = projectionStore.Load(persistentVersion, versionId, 1).ToList();
            projectionCommits.AddRange(loadedCommits);

            var snapshot = new NoSnapshot(versionId, projectionName);

            ProjectionStream stream = new ProjectionStream(versionId, projectionCommits, () => snapshot);
            var queryResult = stream.RestoreFromHistory<ProjectionVersionsHandler>();

            if (queryResult.Success)
                return queryResult.Projection.State.AllVersions;

            return new ProjectionVersions();
        }

        bool IsVersionOutdated(ProjectionVersion version)
        {
            ProjectionVersions versions = GetProjectionVersionsFromStore(version);
            ProjectionVersion liveVersion = versions.GetLive();
            if (ReferenceEquals(null, liveVersion)) return false;

            return liveVersion > version;
        bool IsCanceled(ProjectionVersion version)
        {
            ProjectionVersions versions = GetProjectionVersionsFromStore(version);

            return versions.IsCanceled(version);
        }
    }
}
