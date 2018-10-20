using System.Linq;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using Elders.Cronus.IntegrityValidation;
using System;

namespace Elders.Cronus.EventStore
{
    public sealed class AggregateRepository : IAggregateRepository
    {
        readonly IAggregateRootAtomicAction atomicAction;
        readonly IEventStore eventStore;
        readonly IIntegrityPolicy<EventStream> integrityPolicy;

        public AggregateRepository(IEventStore eventStore, IAggregateRootAtomicAction atomicAction, IIntegrityPolicy<EventStream> integrityPolicy)
        {
            if (eventStore is null) throw new ArgumentNullException(nameof(eventStore));
            if (atomicAction is null) throw new ArgumentNullException(nameof(atomicAction));
            if (integrityPolicy is null) throw new ArgumentNullException(nameof(integrityPolicy));

            this.eventStore = eventStore;
            this.atomicAction = atomicAction;
            this.integrityPolicy = integrityPolicy;
        }

        /// <summary>
        /// Saves the specified aggregate root.
        /// </summary>
        /// <typeparam name="AR">The type of the aggregate root.</typeparam>
        /// <param name="aggregateRoot">The aggregate root.</param>
        /// <exception cref="Elders.Cronus.DomainModeling.AggregateRootException"></exception>
        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            if (ReferenceEquals(null, aggregateRoot.UncommittedEvents) || aggregateRoot.UncommittedEvents.Any() == false)
                return;

            var arCommit = new AggregateCommit(aggregateRoot.State.Id as IBlobId, aggregateRoot.Revision, aggregateRoot.UncommittedEvents.ToList());
            var result = atomicAction.Execute(aggregateRoot.State.Id, aggregateRoot.Revision, () => eventStore.Append(arCommit));

            if (result.IsSuccessful)
            {
                // #prodalzavameNapred
                // #bravoKobra
                // https://www.youtube.com/watch?v=2wWusHu_3w8
            }
            else
            {
                throw new AggregateStateFirstLevelConcurrencyException("Unable to save AR" + Environment.NewLine + arCommit.ToString(), result.Errors.MakeJustOneException());
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
            var integrityResult = integrityPolicy.Apply(eventStream);
            if (integrityResult.IsIntegrityViolated)
                throw new EventStreamIntegrityViolationException("asd");
            eventStream = integrityResult.Output;
            AR aggregateRoot;
            if (eventStream.TryRestoreFromHistory<AR>(out aggregateRoot) == false)
                throw new AggregateLoadException("Unable to load AR with ID=" + id.Urn.Value);

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
            var integrityResult = integrityPolicy.Apply(eventStream);
            if (integrityResult.IsIntegrityViolated)
                return false;
            return eventStream.TryRestoreFromHistory<AR>(out aggregateRoot);
        }
    }
}
