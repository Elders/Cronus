using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;

namespace Elders.Cronus.EventStore
{
    public sealed class AggregateRepository : IAggregateRepository
    {
        private readonly IAggregateRootAtomicAction atomicAction;
        private readonly IEventStore eventStore;

        public AggregateRepository(IEventStore eventStore, IAggregateRootAtomicAction atomicAction)
        {
            this.eventStore = eventStore;
            this.atomicAction = atomicAction;
        }

        /// <summary>
        /// Saves the specified aggregate root.
        /// </summary>
        /// <typeparam name="AR">The type of the aggregate root.</typeparam>
        /// <param name="aggregateRoot">The aggregate root.</param>
        /// <exception cref="Elders.Cronus.DomainModeling.AggregateStateFirstLevelConcurrencyException"></exception>
        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            if (ReferenceEquals(null, aggregateRoot.UncommittedEvents) || aggregateRoot.UncommittedEvents.Count() == 0)
                return;

            var arCommit = new AggregateCommit(aggregateRoot.State.Id, aggregateRoot.Revision, aggregateRoot.UncommittedEvents.ToList());
            var result = atomicAction.Execute(aggregateRoot.State.Id, aggregateRoot.Revision, () => eventStore.Append(arCommit));

            if (result.IsSuccessful)
            {
                // #prodalzavameNapred
                // #bravoKobra
                // https://www.youtube.com/watch?v=2wWusHu_3w8
            }
            else
            {
                throw new AggregateStateFirstLevelConcurrencyException("", result.Errors.MakeJustOneException());
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

        /// <summary>
        /// Loads an aggregate with the specified identifier.
        /// </summary>
        /// <typeparam name="AR">The type of the r.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool TryLoad<AR>(IAggregateRootId id, out AR aggregateRoot) where AR : IAggregateRoot
        {
            aggregateRoot = default(AR);
            EventStream eventStream = eventStore.Load(id);
            return eventStream.TryRestoreFromHistory<AR>(out aggregateRoot);
        }
    }
}
