using System.Collections.Generic;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.EventSourcing
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
