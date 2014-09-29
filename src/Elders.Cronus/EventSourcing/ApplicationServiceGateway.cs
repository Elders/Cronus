using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventSourcing
{

    /// <summary>
    /// Handles changes in aggregates using batches. This implementation works explicitly with IBatchUnitOfWork.
    /// </summary>
    /// <remarks>The class is NOT THREAD SAFE.</remarks>
    public class ApplicationServiceGateway : IApplicationServiceGateway, IAggregateRepository
    {
        private readonly List<IAggregateRoot> aggregates;

        private readonly IAggregateRepository aggregateRepository;

        private readonly IEventStorePersister eventStorePersister;

        public ApplicationServiceGateway(IAggregateRepository aggregateRepository, IEventStorePersister eventStorePersister)
        {
            this.aggregateRepository = aggregateRepository;
            this.eventStorePersister = eventStorePersister;
            aggregates = new List<IAggregateRoot>();
        }

        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            aggregates.Add(aggregateRoot);
        }

        public AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            return aggregateRepository.Load<AR>(id);
        }

        public void CommitChanges(Action<IEvent> publish)
        {
            eventStorePersister.Persist(aggregates);
            aggregates.ForEach(e => e.UncommittedEvents.ForEach(x => publish(x)));
        }
    }
}
