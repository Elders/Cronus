using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.MessageProcessing
{
    public class CronusAggregateRepository : IAggregateRepository
    {
        readonly IAggregateRepository aggregateRepository;
        readonly IPublisher<IEvent> eventPublisher;
        private readonly IPublisher<IPublicEvent> publicEventPublisher;
        private readonly CronusContext context;

        public CronusAggregateRepository(IAggregateRepository repository, IPublisher<IEvent> eventPublisher, IPublisher<IPublicEvent> publicEventPublisher, CronusContext context)
        {
            this.aggregateRepository = repository;
            this.eventPublisher = eventPublisher;
            this.publicEventPublisher = publicEventPublisher;
            this.context = context;
        }

        public ReadResult<AR> Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            return aggregateRepository.Load<AR>(id);
        }

        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            aggregateRepository.Save<AR>(aggregateRoot);

            if (ReferenceEquals(null, aggregateRoot.UncommittedEvents) || aggregateRoot.UncommittedEvents.Any() == false)
                return;

            var events = aggregateRoot.UncommittedEvents.ToList();
            for (int i = 0; i < events.Count; i++)
            {
                var theEvent = events[i];
                var entityEvent = theEvent as EntityEvent;
                if (ReferenceEquals(null, entityEvent) == false)
                    theEvent = entityEvent.Event;


                eventPublisher.Publish(theEvent, BuildHeaders(aggregateRoot, i));
            }

            var publicEvents = aggregateRoot.UncommittedPublicEvents.ToList();
            for (int i = 0; i < publicEvents.Count; i++)
            {
                publicEventPublisher.Publish(publicEvents[i], BuildHeaders(aggregateRoot, i));
            }
        }

        Dictionary<string, string> BuildHeaders(IAggregateRoot aggregateRoot, int eventPosition)
        {
            Dictionary<string, string> messageHeaders = new Dictionary<string, string>();

            messageHeaders.Add(MessageHeader.AggregateRootId, Convert.ToBase64String(aggregateRoot.State.Id.RawId));
            messageHeaders.Add(MessageHeader.AggregateRootRevision, aggregateRoot.Revision.ToString());
            messageHeaders.Add(MessageHeader.AggregateRootEventPosition, eventPosition.ToString());

            foreach (var trace in context.Trace)
            {
                messageHeaders.Add(trace.Key, trace.Value.ToString());
            }

            return messageHeaders;
        }
    }
}
