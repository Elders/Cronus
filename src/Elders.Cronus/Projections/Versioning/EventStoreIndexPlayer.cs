using System;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;
using Elders.Cronus.Projections.Cassandra.EventSourcing;

namespace Elders.Cronus.Projections.Versioning
{
    public class EventStoreIndexPlayer : IProjectionPlayer
    {
        static ILog log = LogProvider.GetLogger(typeof(EventStoreIndexPlayer));

        readonly IEventStorePlayer eventStorePlayer;
        private readonly IProjectionStore projectionStore;
        readonly IProjectionRepository projectionRepository;

        public EventStoreIndexPlayer(IEventStorePlayer eventStorePlayer, IProjectionStore projectionStore, IProjectionRepository projectionRepository)
        {
            this.eventStorePlayer = eventStorePlayer;
            this.projectionStore = projectionStore;
            this.projectionRepository = projectionRepository;
        }

        public void Rebuild(Type projectionType, ProjectionVersion version)
        {
            projectionStore.InitializeProjectionStore(version);

            foreach (var aggregateCommit in eventStorePlayer.LoadAggregateCommits())
            {
                foreach (var @event in aggregateCommit.Events)
                {
                    try
                    {
                        var unwrapedEvent = @event.Unwrap();
                        var rootId = System.Text.Encoding.UTF8.GetString(aggregateCommit.AggregateRootId);
                        var eventOrigin = new EventOrigin(rootId, aggregateCommit.Revision, aggregateCommit.Events.IndexOf(@event), aggregateCommit.Timestamp);

                        projectionRepository.Save(projectionType, @event, eventOrigin);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorException(ex.Message, ex);
                    }
                }
            }
        }
    }
}
