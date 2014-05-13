using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class CassandraEventStorePlayer : IEventStorePlayer
    {
        public IEnumerable<IEvent> GetEventsFromStart(int batchPerQuery = 1)
        {
            return new List<IEvent>();
        }
    }
}