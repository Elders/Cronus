using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore.InMemory
{

    public class InMemoryEventStorePersister : IEventStorePersister
    {
        private InMemoryEventStoreStorage eventStoreStorage;


        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStorePersister"/> class.
        /// </summary>
        /// <param name="eventStoreStorage">The event store storage.</param>
        public InMemoryEventStorePersister(InMemoryEventStoreStorage eventStoreStorage)
        {
            this.eventStoreStorage = eventStoreStorage;
        }

        /// <summary>
        /// Loads all the commits of an aggregate with the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public List<AggregateCommit> Load(IAggregateRootId aggregateId)
        {
            return eventStoreStorage.Seek(aggregateId);
        }


        /// <summary>
        /// Persists the specified aggregate commit.
        /// </summary>
        /// <param name="aggregateCommit">The aggregate commit.</param>
        public void Persist(AggregateCommit aggregateCommit)
        {
            eventStoreStorage.Flush(aggregateCommit);
        }
    }
}