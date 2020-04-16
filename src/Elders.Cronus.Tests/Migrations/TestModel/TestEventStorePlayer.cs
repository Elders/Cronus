using Elders.Cronus.EventStore;
using System;
using System.Collections.Generic;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel
{
    public class TestEventStorePlayer : IEventStorePlayer
    {
        readonly TestEventStore eventStore;
        public TestEventStorePlayer(TestEventStore eventStore)
        {
            if (ReferenceEquals(eventStore, null) == true) throw new ArgumentNullException(nameof(eventStore));
            this.eventStore = eventStore;
        }
        public IEnumerable<AggregateCommit> LoadAggregateCommits(int batchSize = 5000)
        {
            // hack
            return eventStore.Storage;
        }

        public LoadAggregateCommitsResult LoadAggregateCommits(string paginationToken, int batchSize = 5000)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<AggregateCommit> LoadAggregateCommitsAsync()
        {
            throw new NotImplementedException();
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
