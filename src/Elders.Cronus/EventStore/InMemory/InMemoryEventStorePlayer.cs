using System;
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
        public IEnumerable<AggregateCommit> GetFromStart(int batchPerQuery = 1)
        {
            return this.eventStoreStorage.GetOrderedEvents();
        }

        public IEnumerable<AggregateCommit> GetFromStart(DateTime start, DateTime end, int batchPerQuery = 1)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AggregateCommit> LoadAggregateCommits(int batchSize = 100)
        {
            throw new NotImplementedException();
        }
    }
}
