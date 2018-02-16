using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Elders.Cronus.Projections.Snapshotting;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.Projections
{
    public interface IProjectionPlayer { }

    public class ProjectionPlayer : IProjectionPlayer
    {
        static ILog log = LogProvider.GetLogger(typeof(ProjectionPlayer));

        private readonly IEventStore eventStore;
        private readonly IProjectionStore projectionStore;
        private readonly IProjectionRepository projectionRepository;
        private readonly ISnapshotStore snapshotStore;
        private readonly EventTypeIndexForProjections index;
        private readonly IEventStorePlayer eventStorePlayer;

        public ProjectionPlayer(IEventStore eventStore, IProjectionStore projectionStore, IProjectionRepository projectionRepository, ISnapshotStore snapshotStore, EventTypeIndexForProjections index, IEventStorePlayer eventStorePlayer)
        {
            this.eventStore = eventStore;
            this.projectionStore = projectionStore;
            this.projectionRepository = projectionRepository;
            this.snapshotStore = snapshotStore;
            this.index = index;
            this.eventStorePlayer = eventStorePlayer;
        }

        public bool Rebuild(Type projectionType, ProjectionVersion version, DateTime replayUntil)
        {
            int progressCounter = 0;
            log.Info(() => $"Start rebuilding projection `{projectionType.Name}` for version {version}. Deadline is {replayUntil}");

            var projection = FastActivator.CreateInstance(projectionType) as IProjectionDefinition;
            var projectionEventTypes = GetInvolvedEvents(projectionType).ToList();

            projectionStore.InitializeProjectionStore(version);
            snapshotStore.InitializeProjectionSnapshotStore(version);
            var indexState = index.GetIndexState();
            if (indexState.IsPresent() == false)
                return false;
            foreach (var eventType in projectionEventTypes)
            {
                log.Debug(() => $"Rebuilding projection `{projectionType.Name}` for version {version} using eventType `{eventType}`. Deadline is {replayUntil}");

                var indexId = new EventStoreIndexEventTypeId(eventType);
                IEnumerable<ProjectionCommit> indexCommits = index.EnumerateCommitsByEventType(indexId);

                foreach (var indexCommit in indexCommits)
                {
                    progressCounter++;
                    if (progressCounter % 1000 == 0)
                        log.Trace(() => $"Rebuilding projection `{projectionType.Name}` => PROGRESS:{progressCounter} Version:{version} EventType:`{eventType}` Deadline:{replayUntil}.");
                    // if the replay did not finish in time (specified by the AR) we need to abort.
                    if (DateTime.UtcNow >= replayUntil)
                    {
                        log.Info(() => $"Rebuilding projection `{projectionType.Name}` stopped bacause the deadline has been reached. PROGRESS:{progressCounter} Version:{version} EventType:`{eventType}` Deadline:{replayUntil}.");
                        return false;
                    }
                    IAggregateRootId arId = GetAggregateRootId(indexCommit.EventOrigin.AggregateRootId);
                    EventStream stream = eventStore.Load(arId, theId => projectionType.GetBoundedContext().BoundedContextName);

                    foreach (AggregateCommit arCommit in stream.Commits)
                    {
                        for (int i = 0; i < arCommit.Events.Count; i++)
                        {
                            IEvent theEvent = arCommit.Events[i].Unwrap();

                            if (projectionEventTypes.Contains(theEvent.GetType().GetContractId()))
                            {
                                var origin = new EventOrigin(Convert.ToBase64String(arCommit.AggregateRootId), arCommit.Revision, i, arCommit.Timestamp);
                                projectionRepository.Save(projectionType, theEvent, origin); // overwrite
                            }
                        }
                    }
                }
            }
            log.Info(() => $"Finish rebuilding projection `{projectionType.Name}` for version {version}. Deadline was {replayUntil}");
            return true;
        }

        public bool RebuildIndex()
        {
            var indexState = index.GetIndexState();
            if (indexState.IsPresent())
                return true;

            var indexBuilder = index.GetIndexBuilder();
            foreach (var aggregateCommit in eventStorePlayer.LoadAggregateCommits())
            {
                foreach (var @event in aggregateCommit.Events)
                {
                    try
                    {
                        var unwrapedEvent = @event.Unwrap();
                        var rootId = System.Text.Encoding.UTF8.GetString(aggregateCommit.AggregateRootId);
                        var eventOrigin = new EventOrigin(rootId, aggregateCommit.Revision, aggregateCommit.Events.IndexOf(@event), aggregateCommit.Timestamp);
                        indexBuilder.Feed(unwrapedEvent, eventOrigin);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorException(ex.Message, ex);
                    }
                }
            }
            indexBuilder.Complete();
            return true;
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
    }
}
