using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.EventSourcing
{
    public interface IEventStore
    {
        IAggregateRepository AggregateRepository { get; }
        IEventStorePersister Persister { get; }
        IEventStorePlayer Player { get; }
        IEventStoreStorageManager StorageManager { get; }
    }
}