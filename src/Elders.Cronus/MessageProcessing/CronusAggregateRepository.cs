using Elders.Cronus.EventStore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elders.Cronus.MessageProcessing
{
    internal class AggregateRepositoryAndEventPublisher : IAggregateRepository
    {
        readonly AggregateRepository aggregateRepository;
        readonly IPublisher<IEvent> eventPublisher;
        private readonly IPublisher<IPublicEvent> publicEventPublisher;
        private readonly CronusContext context;

        public AggregateRepositoryAndEventPublisher(AggregateRepository repository, IPublisher<IEvent> eventPublisher, IPublisher<IPublicEvent> publicEventPublisher, CronusContext context)
        {
            this.aggregateRepository = repository;
            this.eventPublisher = eventPublisher;
            this.publicEventPublisher = publicEventPublisher;
            this.context = context;
        }

        public Task<ReadResult<AR>> LoadAsync<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            return aggregateRepository.LoadAsync<AR>(id);
        }

        public async Task SaveAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            AggregateCommit aggregateCommit = await aggregateRepository.SaveInternalAsync(aggregateRoot).ConfigureAwait(false);

            if (ReferenceEquals(null, aggregateRoot.UncommittedEvents) || aggregateRoot.UncommittedEvents.Any() == false)
                return;

            var events = aggregateRoot.UncommittedEvents.ToList();
            for (int i = 0; i < events.Count; i++)
            {
                var theEvent = events[i];
                var entityEvent = theEvent as EntityEvent;
                if (ReferenceEquals(null, entityEvent) == false)
                    theEvent = entityEvent.Event;


                eventPublisher.Publish(theEvent, BuildHeaders(aggregateCommit, aggregateRoot, i));
            }

            var publicEvents = aggregateRoot.UncommittedPublicEvents.ToList();
            for (int i = 0; i < publicEvents.Count; i++)
            {
                publicEventPublisher.Publish(publicEvents[i], BuildHeaders(aggregateCommit, aggregateRoot, i));
            }
        }

        Dictionary<string, string> BuildHeaders(AggregateCommit aggregatecommit, IAggregateRoot aggregateRoot, int eventPosition)
        {
            Dictionary<string, string> messageHeaders = new Dictionary<string, string>();

            messageHeaders.Add(MessageHeader.AggregateRootId, aggregateRoot.State.Id.Value);
            messageHeaders.Add(MessageHeader.AggregateRootRevision, aggregateRoot.Revision.ToString());
            messageHeaders.Add(MessageHeader.AggregateRootEventPosition, eventPosition.ToString());
            messageHeaders.Add(MessageHeader.AggregateCommitTimestamp, aggregatecommit.Timestamp.ToString());

            foreach (var trace in context.Trace)
            {
                if (messageHeaders.ContainsKey(trace.Key) == false)
                    messageHeaders.Add(trace.Key, trace.Value.ToString());
            }

            return messageHeaders;
        }
    }

    public sealed class CronusAggregateRepository : IAggregateRepository
    {
        // TODO: This class should be responsible for logging and building the headers.
        readonly IAggregateRepository aggregateRepository;

        public CronusAggregateRepository(IAggregateRepository repository)
        {
            this.aggregateRepository = repository;
        }

        public Task<ReadResult<AR>> LoadAsync<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            return aggregateRepository.LoadAsync<AR>(id);
        }

        public Task SaveAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            return aggregateRepository.SaveAsync(aggregateRoot);
        }
    }
}
