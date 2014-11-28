using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore
{
    public interface IEventStore
    {
        IAggregateRepository AggregateRepository { get; }
        IEventStorePersister Persister { get; }
        IEventStorePlayer Player { get; }
        IEventStoreStorageManager StorageManager { get; }
    }
}