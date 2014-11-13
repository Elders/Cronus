using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.DomainModeling;
using System.Collections.Concurrent;

namespace Elders.Cronus.EventSourcing.InMemory
{
    public class InMemoryEventStorePlayer : IEventStorePlayer
    {
        public IEnumerable<IEvent> GetEventsFromStart(int batchPerQuery = 1)
        {
            return InMemoryEventStoreStorage.EventsForReplay;
        }
    }
}