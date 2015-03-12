using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore
{
    public interface IEventStorePlayer
    {
        IEnumerable<IEvent> GetEventsFromStart(int batchPerQuery = 1);
    }
}