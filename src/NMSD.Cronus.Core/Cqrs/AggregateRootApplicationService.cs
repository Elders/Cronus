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
            var state = EventStore.LoadAggregateState("Collaboration", id.Id);
            AR aggregateRoot = AggregateRootFactory.Build<AR>(state);
            updateAr(aggregateRoot);

            EventStore.Persist(aggregateRoot.UncommittedEvents);
            aggregateRoot.State.Version++;
            EventStore.TakeSnapshot(aggregateRoot.State);
            foreach (var uncommittedEvent in aggregateRoot.UncommittedEvents)
            {
                EventBus.Publish(uncommittedEvent);
            }
        }

        protected void CreateAggregate(AR aggregateRoot)
        {
            EventStore.Persist(aggregateRoot.UncommittedEvents);
            aggregateRoot.State.Version++;
            EventStore.TakeSnapshot(aggregateRoot.State);
            foreach (var uncommittedEvent in aggregateRoot.UncommittedEvents)
            {
                EventBus.Publish(uncommittedEvent);
            }
        }
    }
}
