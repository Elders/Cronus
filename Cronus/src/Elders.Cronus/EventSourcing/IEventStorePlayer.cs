using System.Collections.Generic;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.EventSourcing
{
    public interface IEventStorePlayer
    {
        IEnumerable<IEvent> GetEventsFromStart(int batchPerQuery = 1);
    }
}