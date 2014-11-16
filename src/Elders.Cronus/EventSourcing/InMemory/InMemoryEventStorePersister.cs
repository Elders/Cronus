using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.DomainModeling;
using System.Collections.Concurrent;

namespace Elders.Cronus.EventSourcing.InMemory
{
    public class InMemoryEventStorePersister : IEventStorePersister
    {
        private InMemoryEventStoreStorage eventStoreStorage;

        public InMemoryEventStorePersister(InMemoryEventStoreStorage eventStoreStorage)
        {
            this.eventStoreStorage = eventStoreStorage;
        }

        public void Persist(List<IAggregateRoot> aggregates)
        {
            foreach (var aggregate in aggregates)
            {
                if (eventStoreStorage.EventsStreams.ContainsKey(aggregate.State.Id))
                {
                    var evnts = eventStoreStorage.EventsStreams[aggregate.State.Id];

                    if (evnts == null)
                        eventStoreStorage.EventsStreams[aggregate.State.Id] = new ConcurrentQueue<EventsStream>();

                    var eventsStream = new EventsStream();
                    eventsStream.Version = aggregate.State.Version;

                    foreach (var evnt in aggregate.UncommittedEvents)
                    {
                        eventsStream.Events.Enqueue(evnt);
                        eventStoreStorage.EventsForReplay.Enqueue(evnt);
                    }

                    eventStoreStorage.EventsStreams[aggregate.State.Id].Enqueue(eventsStream);
                }
                else
                {
                    var evnts = new ConcurrentQueue<EventsStream>();

                    var eventsStream = new EventsStream();
                    eventsStream.Version = aggregate.State.Version;

                    foreach (var evnt in aggregate.UncommittedEvents)
                    {
                        eventsStream.Events.Enqueue(evnt);
                        eventStoreStorage.EventsForReplay.Enqueue(evnt);
                    }

                    evnts.Enqueue(eventsStream);

                    eventStoreStorage.EventsStreams.TryAdd(aggregate.State.Id, evnts);
                }
            }
        }
    }
}