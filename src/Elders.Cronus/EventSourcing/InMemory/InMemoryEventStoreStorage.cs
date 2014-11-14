using Elders.Cronus.DomainModeling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.EventSourcing.InMemory
{
    public class InMemoryEventStoreStorage : IDisposable
    {
        private static ConcurrentDictionary<IAggregateRootId, ConcurrentQueue<EventsStream>> eventsStreams;
        public ConcurrentDictionary<IAggregateRootId, ConcurrentQueue<EventsStream>> EventsStreams
        {
            get
            {
                if (eventsStreams == null)
                    eventsStreams = new ConcurrentDictionary<IAggregateRootId, ConcurrentQueue<EventsStream>>();

                return eventsStreams;
            }
        }

        private static ConcurrentQueue<IEvent> eventsForReplay;
        public ConcurrentQueue<IEvent> EventsForReplay
        {
            get
            {
                if (eventsForReplay == null)
                    eventsForReplay = new ConcurrentQueue<IEvent>();

                return eventsForReplay;
            }
        }

        public void Dispose()
        {
            if (eventsStreams != null)
                eventsStreams = null;

            if (eventsForReplay != null)
                eventsForReplay = null;
        }
    }

    public class EventsStream
    {
        public EventsStream()
        {
            Events = new ConcurrentQueue<IEvent>();
        }

        public int Version { get; set; }

        public ConcurrentQueue<IEvent> Events { get; set; }
    }
}