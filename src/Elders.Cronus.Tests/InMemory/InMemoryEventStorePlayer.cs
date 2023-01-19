using Elders.Cronus.EventStore.Index;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.InMemory
{
    public class InMemoryEventStorePlayer : IEventStorePlayer
    {
        private InMemoryEventStoreStorage eventStoreStorage;

        public InMemoryEventStorePlayer(InMemoryEventStoreStorage eventStoreStorage)
        {
            this.eventStoreStorage = eventStoreStorage;
        }

        public Task EnumerateEventStore(PlayerOperator @operator, PlayerOptions replayOptions)
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<AggregateCommit> LoadAggregateCommitsAsync(int batchSize = 5000)
        {
            foreach (var @event in eventStoreStorage.GetOrderedEvents())
                yield return @event;
        }
    }
}
