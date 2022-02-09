using System.Linq;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using Elders.Cronus.IntegrityValidation;
using System;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore
{
    public sealed class AggregateRepository : IAggregateRepository
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(AggregateRepository));

        readonly IAggregateRootAtomicAction atomicAction;
        readonly IEventStore eventStore;
        readonly IIntegrityPolicy<EventStream> integrityPolicy;

        public AggregateRepository(EventStoreFactory eventStoreFactory, IAggregateRootAtomicAction atomicAction, IIntegrityPolicy<EventStream> integrityPolicy)
        {
            if (eventStoreFactory is null) throw new ArgumentNullException(nameof(eventStoreFactory));
            if (atomicAction is null) throw new ArgumentNullException(nameof(atomicAction));
            if (integrityPolicy is null) throw new ArgumentNullException(nameof(integrityPolicy));

            this.eventStore = eventStoreFactory.GetEventStore();
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
            SaveInternal(aggregateRoot);
        }

        /// <summary>
        /// Loads an aggregate with the specified identifier.
        /// </summary>
        /// <typeparam name="AR">The type of the r.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public ReadResult<AR> Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            EventStream eventStream = eventStore.Load(id);
            var integrityResult = integrityPolicy.Apply(eventStream);
            if (integrityResult.IsIntegrityViolated)
                throw new EventStreamIntegrityViolationException($"AR integrity is violated for ID={id.Value}");
            eventStream = integrityResult.Output;
            AR aggregateRoot;
            if (eventStream.TryRestoreFromHistory<AR>(out aggregateRoot) == false)
                return ReadResult<AR>.WithNotFoundHint($"Unable to load AR with ID={id.Value}");

            return new ReadResult<AR>(aggregateRoot);
        }

        internal AggregateCommit SaveInternal<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            if (aggregateRoot.UncommittedEvents is null || aggregateRoot.UncommittedEvents.Any() == false)
            {
                if (aggregateRoot.UncommittedPublicEvents is not null && aggregateRoot.UncommittedPublicEvents.Any())
                {
                    throw new Exception("Public events cannot be applied by themselves. If you wanna publish a public event then you need to have an IEvent in the same revision. It is not ok to update aggregate state with public events!");
                }

                return default;
            }

            var arCommit = new AggregateCommit(aggregateRoot.State.Id as IBlobId, aggregateRoot.Revision, aggregateRoot.UncommittedEvents.ToList(), aggregateRoot.UncommittedPublicEvents.ToList());
            var result = atomicAction.Execute(aggregateRoot.State.Id, aggregateRoot.Revision, () => eventStore.Append(arCommit));

            if (result.IsSuccessful)
            {
                // #prodalzavameNapred
                // #bravoKobra
                // https://www.youtube.com/watch?v=2wWusHu_3w8

                return arCommit;
            }
            else
            {
                throw new AggregateStateFirstLevelConcurrencyException($"Unable to save AR {Environment.NewLine}{arCommit.ToString()}", result.Errors.MakeJustOneException());
            }
        }
    }
}
