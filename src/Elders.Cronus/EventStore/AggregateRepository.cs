using System.Linq;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore
{
    public sealed class AggregateRepository : IAggregateRepository
    {
        private readonly IAggregateRevisionService revisionService;
        private readonly IEventStore eventStore;
        private readonly IPublisher<IEvent> eventPublisher;

        public AggregateRepository(IEventStore eventStore, IPublisher<IEvent> eventPublisher, IAggregateRevisionService revisionService)
        {
            this.eventStore = eventStore;
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
            eventStore.Append(arCommit);
            PublishNewEvents(arCommit);
        }

        /// <summary>
        /// Loads an aggregate with the specified identifier.
        /// </summary>
        /// <typeparam name="AR">The type of the r.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            EventStream eventStream = eventStore.Load(id);
            AR aggregateRoot = eventStream.RestoreFromHistory<AR>();
            return aggregateRoot;
        }

        public void PublishNewEvents(AggregateCommit aggregateCommit)
        {
            aggregateCommit.Events.ForEach(e => eventPublisher.Publish(e));
        }
    }

}