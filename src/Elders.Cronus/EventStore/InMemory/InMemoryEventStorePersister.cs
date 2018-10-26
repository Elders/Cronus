namespace Elders.Cronus.EventStore.InMemory
{

    public class InMemoryEventStore : IEventStore
    {
        private InMemoryEventStoreStorage eventStoreStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStore"/> class.
        /// </summary>
        /// <param name="eventStoreStorage">The event store storage.</param>
        public InMemoryEventStore(InMemoryEventStoreStorage eventStoreStorage)
        {
            this.eventStoreStorage = eventStoreStorage;
        }

        /// <summary>
        /// Loads all the commits of an aggregate with the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public EventStream Load(IAggregateRootId aggregateId)
        {
            return new EventStream(eventStoreStorage.Seek(aggregateId));
        }


        /// <summary>
        /// Persists the specified aggregate commit.
        /// </summary>
        /// <param name="aggregateCommit">The aggregate commit.</param>
        public void Append(AggregateCommit aggregateCommit)
        {
            eventStoreStorage.Flush(aggregateCommit);
        }
    }
}
