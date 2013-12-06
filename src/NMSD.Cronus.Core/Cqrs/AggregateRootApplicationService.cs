using System;
using Cronus.Core.Eventing;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Messaging;
using System.Runtime.Remoting.Messaging;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootApplicationService : IMessageHandler
    {
        InMemoryEventStore EventStore { get; set; }

        IPublisher<IEvent> EventPublisher { get; set; }
    }
    public class AggregateRootApplicationService<AR> : IAggregateRootApplicationService where AR : IAggregateRoot
    {
        public InMemoryEventStore EventStore { get; set; }
        public IPublisher<IEvent> EventPublisher { get; set; }

        protected void UpdateAggregate(IAggregateRootId id, Action<AR> updateAr)
        {
            var state = EventStore.LoadAggregateState("Collaboration", id.Id);
            AR aggregateRoot = AggregateRootFactory.Build<AR>(state);
            updateAr(aggregateRoot);

            //EventStore.Persist(aggregateRoot.UncommittedEvents);
            //aggregateRoot.State.Version++;
            //EventStore.TakeSnapshot(aggregateRoot.State);
            //foreach (var uncommittedEvent in aggregateRoot.UncommittedEvents)
            //{
            //    EventPublisher.Publish(uncommittedEvent);
            //}
        }

        protected void CreateAggregate(AR aggregateRoot)
        {
            EventStore.Persist(aggregateRoot.UncommittedEvents);
            aggregateRoot.State.Version++;
            EventStore.TakeSnapshot(aggregateRoot.State);
            foreach (var uncommittedEvent in aggregateRoot.UncommittedEvents)
            {
                EventPublisher.Publish(uncommittedEvent);
            }
        }
    }
}
