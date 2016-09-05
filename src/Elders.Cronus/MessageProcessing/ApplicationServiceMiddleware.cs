using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessing
{
    public class ApplicationServiceMiddleware : MessageHandlerMiddleware
    {
        public ApplicationServiceMiddleware(IHandlerFactory factory, IAggregateRepository aggregateRepository, IPublisher<IEvent> eventPublisher) : base(factory)
        {
            BeginHandle.Use((execution) =>
            {
                var repo = new CronusAggregateRepository(aggregateRepository, eventPublisher, execution.Context.CronusMessage);
                execution.Context.HandlerInstance.AssignPropertySafely<IAggregateRootApplicationService>(x => x.Repository = repo);
            });
        }

        class CronusAggregateRepository : IAggregateRepository
        {
            readonly IAggregateRepository aggregateRepository;
            readonly IPublisher<IEvent> eventPublisher;
            CronusMessage cronusMessage;
            public CronusAggregateRepository(IAggregateRepository repository, IPublisher<IEvent> eventPublisher, CronusMessage cronusMessage)
            {
                this.cronusMessage = cronusMessage;
                this.aggregateRepository = repository;
                this.eventPublisher = eventPublisher;
            }

            public AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
            {
                return aggregateRepository.Load<AR>(id);
            }

            public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
            {
                aggregateRepository.Save<AR>(aggregateRoot);

                var events = aggregateRoot.UncommittedEvents.ToList();
                for (int i = 0; i < events.Count; i++)
                {
                    var theEvent = events[i];
                    var entityEvent = theEvent as EntityEvent;
                    if (ReferenceEquals(null, entityEvent) == false)
                        theEvent = entityEvent.Event;


                    eventPublisher.Publish(theEvent, BuildHeaders(aggregateRoot, i, cronusMessage));
                }
            }

            public bool TryLoad<AR>(IAggregateRootId id, out AR aggregateRoot) where AR : IAggregateRoot
            {
                return aggregateRepository.TryLoad<AR>(id, out aggregateRoot);
            }

            Dictionary<string, string> BuildHeaders(IAggregateRoot aggregateRoot, int eventPosition, CronusMessage triggeredBy)
            {
                Dictionary<string, string> messageHeaders = new Dictionary<string, string>();

                messageHeaders.Add(MessageHeader.AggregateRootId, aggregateRoot.State.Id.ToString());
                messageHeaders.Add(MessageHeader.AggregateRootRevision, aggregateRoot.Revision.ToString());
                messageHeaders.Add(MessageHeader.PublishTimestamp, DateTime.UtcNow.ToFileTimeUtc().ToString());
                messageHeaders.Add(MessageHeader.AggregateRootEventPosition, eventPosition.ToString());

                messageHeaders.Add(MessageHeader.CausationId, triggeredBy.MessageId);
                messageHeaders.Add(MessageHeader.CorelationId, triggeredBy.CorelationId);

                return messageHeaders;
            }
        }
    }
}
