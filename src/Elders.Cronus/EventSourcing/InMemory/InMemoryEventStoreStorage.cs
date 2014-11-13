using Elders.Cronus.DomainModeling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.EventSourcing.InMemory
{
    internal static class InMemoryEventStoreStorage
    {
        public static ConcurrentDictionary<IAggregateRootId, ConcurrentQueue<EventsStream>> EventsStreams = new ConcurrentDictionary<IAggregateRootId, ConcurrentQueue<EventsStream>>();
        public static ConcurrentQueue<IEvent> EventsForReplay = new ConcurrentQueue<IEvent>();
    }

    internal class EventsStream
    {
        public EventsStream()
        {
            Events = new ConcurrentQueue<IEvent>();
        }

        public int Version { get; set; }

        public ConcurrentQueue<IEvent> Events { get; set; }
    }
}