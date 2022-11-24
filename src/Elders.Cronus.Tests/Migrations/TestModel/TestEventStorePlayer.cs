using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using System;
using System.Collections.Generic;
using System.Threading;
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

        public Task<IEvent> LoadEventWithRebuildProjectionAsync(IndexRecord indexRecord)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<IPublicEvent> LoadPublicEventsAsync(ReplayOptions replayOptions, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<IPublicEvent> LoadPublicEventsAsync(ReplayOptions replayOptions, Action<ReplayOptions> notifyProgress = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
