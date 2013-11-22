using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.EventStoreEngine;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootApplicationService
    {
        InMemoryEventStore EventStore { get; set; }

        InMemoryEventBus EventBus { get; set; }
    }
    public class AggregateRootApplicationService<AR> : IAggregateRootApplicationService where AR : IAggregateRoot
    {
        public InMemoryEventStore EventStore { get; set; }
        public InMemoryEventBus EventBus { get; set; }

        protected void UpdateAggregate(IAggregateRootId id, Action<AR> updateAr)
        {
            //AR aggregateRoot = AggregateRootFactory.BuildFromHistory<AR>(events);
            //updateAr(aggregateRoot);
            //foreach (var uncommittedEvent in aggregateRoot.UncommittedEvents)
            //{
            //    EventBus.Publish(uncommittedEvent);
            //}
        }

        protected void CreateAggregate(AR aggregateRoot)
        {
            EventStore.Persist(aggregateRoot.UncommittedEvents);
            foreach (var uncommittedEvent in aggregateRoot.UncommittedEvents)
            {
                EventBus.Publish(uncommittedEvent);
            }
        }
    }
}
