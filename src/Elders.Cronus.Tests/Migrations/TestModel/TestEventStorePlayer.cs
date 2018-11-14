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
        public IEnumerable<AggregateCommit> LoadAggregateCommits(int batchSize = 100)
        {
            // hack
            return eventStore.Storage;
        }
    }
}
