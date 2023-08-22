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
        private readonly ICronusContextAccessor contextAccessor;

        public AggregateRepositoryAndEventPublisher(AggregateRepository repository, IPublisher<IEvent> eventPublisher, IPublisher<IPublicEvent> publicEventPublisher, ICronusContextAccessor contextAccessor)
        {
            this.aggregateRepository = repository;
            this.eventPublisher = eventPublisher;
            this.publicEventPublisher = publicEventPublisher;
            this.contextAccessor = contextAccessor;
        }

        public Task<ReadResult<AR>> LoadAsync<AR>(AggregateRootId id) where AR : IAggregateRoot
        {
            return aggregateRepository.LoadAsync<AR>(id);
        }

        public async Task SaveAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            AggregateCommit aggregateCommit = await aggregateRepository.SaveInternalAsync(aggregateRoot).ConfigureAwait(false);

            if (aggregateRoot.UncommittedEvents is null || aggregateRoot.UncommittedEvents.Any() == false)
                return;

            int position = -1;
            foreach (IEvent theEvent in aggregateRoot.UncommittedEvents)
            {
                if (theEvent is EntityEvent entityEvent)
                {
                    eventPublisher.Publish(entityEvent.Event, BuildHeaders(aggregateCommit, aggregateRoot, ++position));
                }
                else
                {
                    eventPublisher.Publish(theEvent, BuildHeaders(aggregateCommit, aggregateRoot, ++position));
                }
            }
            position += 5;
            foreach (IPublicEvent publicEvent in aggregateRoot.UncommittedPublicEvents)
            {
                publicEventPublisher.Publish(publicEvent, BuildHeaders(aggregateCommit, aggregateRoot, position++));
            }
        }

        Dictionary<string, string> BuildHeaders(AggregateCommit aggregatecommit, IAggregateRoot aggregateRoot, int eventPosition)
        {
            Dictionary<string, string> messageHeaders = new Dictionary<string, string>();

            messageHeaders.Add(MessageHeader.AggregateRootId, aggregateRoot.State.Id.Value);
            messageHeaders.Add(MessageHeader.AggregateRootRevision, aggregateRoot.Revision.ToString());
            messageHeaders.Add(MessageHeader.AggregateRootEventPosition, eventPosition.ToString());
            messageHeaders.Add(MessageHeader.AggregateCommitTimestamp, aggregatecommit.Timestamp.ToString());

            foreach (var trace in contextAccessor.CronusContext.Trace)
            {
                if (messageHeaders.ContainsKey(trace.Key) == false)
                    messageHeaders.Add(trace.Key, trace.Value.ToString());
            }

            return messageHeaders;
        }
    }
}
