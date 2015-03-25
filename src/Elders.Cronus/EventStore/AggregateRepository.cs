using System.Linq;
using Elders.Cronus.DomainModeling;
using System;

namespace Elders.Cronus.EventStore
{
    public sealed class AggregateRepository : IAggregateRepository
    {
        private readonly IAggregateRootAtomicAction revisionService;
        private readonly IEventStore eventStore;

        public AggregateRepository(IEventStore eventStore, IAggregateRootAtomicAction revisionService)
        {
            this.eventStore = eventStore;
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

            AggregateCommit arCommit = new AggregateCommit(aggregateRoot.State.Id, aggregateRoot.Revision, aggregateRoot.UncommittedEvents.ToList());

            Exception error;
            bool success = revisionService.AtomicAction(
                aggregateRoot.State.Id,
                aggregateRoot.Revision,
                () => eventStore.Append(arCommit),
                out error);

            if (success == false)
                throw new AggregateStateFirstLevelConcurrencyException("", error);
            else
            {
                //  #prodalzavameNapred
                //  #bravoKobra
            }
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
    }
}
