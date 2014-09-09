using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventSourcing
{
    public class EventStream : IEventStream
    {
        public EventStream()
        {
            Events = new List<IEvent>();
            Snapshots = new List<IAggregateRootState>();
        }

        public List<IEvent> Events { get; private set; }

        public List<IAggregateRootState> Snapshots { get; private set; }
    }
}
