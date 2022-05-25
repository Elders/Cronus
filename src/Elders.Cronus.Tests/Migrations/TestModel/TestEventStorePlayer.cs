﻿using Elders.Cronus.EventStore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async IAsyncEnumerable<AggregateCommit> LoadAggregateCommitsAsync(int batchSize = 5000)
        {
            // hack
            foreach (var @event in eventStore.Storage)
                yield return @event;
        }

        public Task<LoadAggregateCommitsResult> LoadAggregateCommitsAsync(string paginationToken, int batchSize = 5000)
        {
            throw new NotImplementedException();
        }

        public Task<LoadAggregateCommitsResult> LoadAggregateCommitsAsync(ReplayOptions replayOptions)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<AggregateCommitRaw> LoadAggregateCommitsRawAsync(int batchSize = 5000)
        {
            throw new NotImplementedException();
        }
    }
}
