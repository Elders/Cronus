using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore
{
    public sealed class AggregateRepository : IAggregateRepository
    {
        private readonly IAggregateRevisionService revisionService;
        private readonly IEventStorePersister persister;
        private readonly IPublisher<IEvent> eventPublisher;

        public AggregateRepository(IEventStorePersister persister, IPublisher<IEvent> eventPublisher, IAggregateRevisionService revisionService)
        {
            this.persister = persister;
            this.eventPublisher = eventPublisher;
            this.revisionService = revisionService;
        }


        /// <summary>
        /// Saves the specified aggregate root.
        /// </summary>
        /// <typeparam name="AR">The type of the aggregate root.</typeparam>
        /// <param name="aggregateRoot">The aggregate root.</param>
        /// <exception cref="Elders.Cronus.DomainModeling.AggregateStateFirstLevelConcurrencyException"></exception>
        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            if (aggregateRoot.UncommittedEvents == null || aggregateRoot.UncommittedEvents.Count() == 0)
                return;

            int reservedVersion = revisionService.ReserveRevision(aggregateRoot.State.Id, aggregateRoot.Revision);
            if (reservedVersion != aggregateRoot.Revision)
            {
                throw new AggregateStateFirstLevelConcurrencyException();
            }

            AggregateCommit arCommit = new AggregateCommit(aggregateRoot.State.Id, aggregateRoot.Revision, aggregateRoot.UncommittedEvents.ToList());
            persister.Persist(arCommit);
            PublishNewEvents(aggregateRoot);
        }

        /// <summary>
        /// Loads an aggregate with the specified identifier.
        /// </summary>
        /// <typeparam name="AR">The type of the r.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            List<AggregateCommit> commits = persister.Load(id);
            AR aggregateRoot = Build<AR>(commits);
            return aggregateRoot;
        }

        private TAggregateRoot Build<TAggregateRoot>(List<AggregateCommit> commits) where TAggregateRoot : IAggregateRoot
        {
            var ar = (TAggregateRoot)FastActivator.CreateInstance(typeof(TAggregateRoot), true);
            var events = commits.SelectMany(x => x.Events).ToList();
            ar.BuildStateFromHistory(events, commits.Last().Revision);
            return ar;
        }


        /// <summary>
        /// Publishes the new events.
        /// </summary>
        /// <typeparam name="AR">The type of the r.</typeparam>
        /// <param name="aggregateRoot">The aggregate root.</param>
        public void PublishNewEvents<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            aggregateRoot.UncommittedEvents.ToList().ForEach(e => eventPublisher.Publish(e));
        }
    }
}