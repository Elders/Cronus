using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore.InMemory
{
    public class InMemoryEventStore : IEventStore
    {
        public InMemoryEventStore(IAggregateRepository aggregateRepository, IEventStorePersister persister, IEventStorePlayer player, IEventStoreStorageManager storageManager)
        {
            AggregateRepository = aggregateRepository;
            Persister = persister;
            Player = player;
            StorageManager = storageManager;
        }

        public IAggregateRepository AggregateRepository { get; private set; }

        public IEventStorePersister Persister { get; private set; }

        public IEventStorePlayer Player { get; private set; }

        public IEventStoreStorageManager StorageManager { get; private set; }
    }
}