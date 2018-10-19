using Elders.Cronus.EventStore.Index;
using System.Collections.Generic;

namespace Elders.Cronus.EventStore
{
    public interface IEventStoreFactory
    {
        IEventStore GetEventStore(string tenant);
        IEventStorePlayer GetEventStorePlayer(string tenant);
        EventStoreIndex GetEventStoreIndex(string tenant);

        IEnumerable<IEventStore> GetEventStores();
        IEnumerable<IEventStorePlayer> GetEventStorePlayers();
    }
}
