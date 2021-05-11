using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.EventStore.InMemory
{
    public class InMemoryEventStorePlayer : IEventStorePlayer
    {
        private InMemoryEventStoreStorage eventStoreStorage;

        public InMemoryEventStorePlayer(InMemoryEventStoreStorage eventStoreStorage)
        {
            this.eventStoreStorage = eventStoreStorage;
        }

        public IEnumerable<AggregateCommit> LoadAggregateCommits(int batchSize = 5000)
        {
            return eventStoreStorage.GetOrderedEvents();
        }

        public LoadAggregateCommitsResult LoadAggregateCommits(string paginationToken, int batchSize = 5000)
        {
            return new LoadAggregateCommitsResult
            {
                Commits = eventStoreStorage.GetOrderedEvents().ToList(),
                PaginationToken = paginationToken
            };
        }

        public LoadAggregateCommitsResult LoadAggregateCommits(ReplayOptions replayOptions)
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<AggregateCommit> LoadAggregateCommitsAsync()
        {
            foreach (var @event in LoadAggregateCommits())
                yield return @event;
        }

        public IEnumerable<AggregateCommitRaw> LoadAggregateCommitsRaw(int batchSize = 5000)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<AggregateCommitRaw> LoadAggregateCommitsRawAsync()
        {
            throw new NotImplementedException();
        }
    }
}
