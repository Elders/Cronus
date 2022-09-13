using Elders.Cronus.EventStore.Index;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async IAsyncEnumerable<AggregateCommit> LoadAggregateCommitsAsync(int batchSize = 5000)
        {
            foreach (var @event in eventStoreStorage.GetOrderedEvents())
                yield return @event;
        }

        public Task<LoadAggregateCommitsResult> LoadAggregateCommitsAsync(ReplayOptions replayOptions)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<AggregateCommitRaw> LoadAggregateCommitsRawAsync(int batchSize = 5000)
        {
            throw new NotImplementedException();
        }

        public Task<IEvent> LoadEventWithRebuildProjectionAsync(IndexRecord indexRecord)
        {
            throw new NotImplementedException();
        }

        Task<LoadAggregateCommitsResult> IEventStorePlayer.LoadAggregateCommitsAsync(string paginationToken, int batchSize)
        {
            LoadAggregateCommitsResult aggregateCommit = new LoadAggregateCommitsResult
            {
                Commits = eventStoreStorage.GetOrderedEvents().ToList(),
                PaginationToken = paginationToken
            };

            return Task.FromResult(aggregateCommit);
        }
    }
}
