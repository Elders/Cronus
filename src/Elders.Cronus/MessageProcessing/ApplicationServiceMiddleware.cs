using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessing
{
    public class ApplicationServiceMiddleware : MessageHandlerMiddleware
    {
        public ApplicationServiceMiddleware(IHandlerFactory factory, IAggregateRepository aggregateRepository, IPublisher<IEvent> eventPublisher) : base(factory)
        {
            BeginHandle.Use((execution) =>
            {
                IPublisher<IEvent> cronusEventPublisher = new CronusPublisher<IEvent>(eventPublisher, execution.Context.CronusMessage);
                var repo = new CronusAggregateRepository(aggregateRepository, cronusEventPublisher);
                execution.Context.HandlerInstance.AssignPropertySafely<IAggregateRootApplicationService>(x => x.Repository = repo);
            });
        }

        class CronusAggregateRepository : IAggregateRepository
        {
            readonly IAggregateRepository aggregateRepository;
            readonly IPublisher<IEvent> eventPublisher;

            public CronusAggregateRepository(IAggregateRepository repository, IPublisher<IEvent> eventPublisher)
            {
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

                    eventPublisher.Publish(theEvent, BuildHeaders(aggregateRoot, i));
                }
            }

            public bool TryLoad<AR>(IAggregateRootId id, out AR aggregateRoot) where AR : IAggregateRoot
            {
                return aggregateRepository.TryLoad<AR>(id, out aggregateRoot);
            }

            Dictionary<string, string> BuildHeaders(IAggregateRoot aggregateRoot, int eventPosition)
            {
                Dictionary<string, string> messageHeaders = new Dictionary<string, string>();

                messageHeaders.Add(MessageHeader.AggregateRootId, aggregateRoot.State.Id.ToString());
                messageHeaders.Add(MessageHeader.AggregateRootRevision, aggregateRoot.Revision.ToString());
                messageHeaders.Add(MessageHeader.AggregateRootEventPosition, eventPosition.ToString());

                return messageHeaders;
            }
        }
    }
}
