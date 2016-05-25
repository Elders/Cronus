using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessingMiddleware
{
    public class ApplicationServiceMiddleware : MessageHandlerMiddleware
    {
        private readonly IAggregateRepository aggregateRepository;

        public ApplicationServiceMiddleware(IHandlerFactory factory, IAggregateRepository aggregateRepository, IPublisher<IEvent> eventPublisher) : base(factory)
        {
            this.aggregateRepository = new RepositoryProxy(aggregateRepository, eventPublisher);
            BeginHandle.Next((context, execution) =>
            {
                context.HandlerInstance.AssignPropertySafely<IAggregateRootApplicationService>(x => x.Repository = this.aggregateRepository);
            });
        }

        class RepositoryProxy : IAggregateRepository
        {
            private readonly IAggregateRepository aggregateRepository;
            private readonly IPublisher<IEvent> eventPublisher;

            public RepositoryProxy(IAggregateRepository repository, IPublisher<IEvent> eventPublisher)
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

            private Dictionary<string, string> BuildHeaders(IAggregateRoot aggregateRoot, int eventPosition)
            {
                Dictionary<string, string> messageHeaders = new Dictionary<string, string>();

                messageHeaders.Add("ar_id", aggregateRoot.State.Id.ToString());
                messageHeaders.Add("ar_revision", aggregateRoot.Revision.ToString());
                messageHeaders.Add("publish_timestamp", DateTime.UtcNow.ToString());
                messageHeaders.Add("event_position", eventPosition.ToString());

                return messageHeaders;
            }
        }
    }
}
