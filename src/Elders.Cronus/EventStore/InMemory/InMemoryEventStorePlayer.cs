using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore.InMemory
{
    public class InMemoryEventStorePlayer : IEventStorePlayer
    {
        private InMemoryEventStoreStorage eventStoreStorage;

        public InMemoryEventStorePlayer(InMemoryEventStoreStorage eventStoreStorage)
        {
            this.eventStoreStorage = eventStoreStorage;
        }


        /// <summary>
        /// Gets the events from start.
        /// </summary>
        /// <param name="batchPerQuery">The batch per query.</param>
        /// <returns></returns>
        public IEnumerable<IEvent> GetEventsFromStart(int batchPerQuery = 1)
        {
            return this.eventStoreStorage.GetOrderedEvents();
        }
    }
}