using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.EventSourcing
{

    /// <summary>
    /// Handles changes in aggregates using batches. This implementation works explicitly with IBatchScope.
    /// </summary>
    /// <remarks>The class is NOT THREAD SAFE.</remarks>
    public class ApplicationServiceGateway : IApplicationServiceGateway, IAggregateRepository
    {
        private readonly List<IDomainMessageCommit> commits;

        private readonly IAggregateRepository aggregateRepository;

        private readonly IEventStorePersister eventStorePersister;

        public ApplicationServiceGateway(IAggregateRepository aggregateRepository, IEventStorePersister eventStorePersister)
        {
            this.aggregateRepository = aggregateRepository;
            this.eventStorePersister = eventStorePersister;
            commits = new List<IDomainMessageCommit>();
        }

        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            var dmc = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents);
            commits.Add(dmc);
        }

        public AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            return aggregateRepository.Load<AR>(id);
        }

        public void CommitChanges(Action<IEvent> publish)
        {
            eventStorePersister.Persist(commits);
            commits.ForEach(e => e.Events.ForEach(x => publish(x)));
        }
    }
}
