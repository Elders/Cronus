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
        public void Persist(List<IAggregateRoot> aggregates)
        {
            foreach (var aggregate in aggregates)
            {
                if (InMemoryEventStoreStorage.EventsStreams.ContainsKey(aggregate.State.Id))
                {
                    var evnts = InMemoryEventStoreStorage.EventsStreams[aggregate.State.Id];

                    if (evnts == null)
                        InMemoryEventStoreStorage.EventsStreams[aggregate.State.Id] = new ConcurrentQueue<EventsStream>();

                    var eventsStream = new EventsStream();
                    eventsStream.Version = aggregate.State.Version;

                    foreach (var evnt in aggregate.UncommittedEvents)
                    {
                        eventsStream.Events.Enqueue(evnt);
                        InMemoryEventStoreStorage.EventsForReplay.Enqueue(evnt);
                    }

                    InMemoryEventStoreStorage.EventsStreams[aggregate.State.Id].Enqueue(eventsStream);
                }
                else
                {
                    var evnts = new ConcurrentQueue<EventsStream>();

                    var eventsStream = new EventsStream();
                    eventsStream.Version = aggregate.State.Version;

                    foreach (var evnt in aggregate.UncommittedEvents)
                    {
                        eventsStream.Events.Enqueue(evnt);
                        InMemoryEventStoreStorage.EventsForReplay.Enqueue(evnt);
                    }

                    evnts.Enqueue(eventsStream);

                    InMemoryEventStoreStorage.EventsStreams.TryAdd(aggregate.State.Id, evnts);
                }
            }
        }
    }
}