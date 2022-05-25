﻿using Elders.Cronus.EventStore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel
{
    public class TestEventStore : IEventStore
    {
        public IList<AggregateCommit> Storage { get; private set; }

        public TestEventStore()
        {
            Storage = new List<AggregateCommit>();
        }

        public Task AppendAsync(AggregateCommit aggregateCommit)
        {
            Storage.Add(aggregateCommit);

            return Task.CompletedTask;
        }


        public Task AppendAsync(AggregateCommitRaw aggregateCommitRaw)
        {
            return Task.FromException(new System.NotImplementedException());
        }
        public Task<EventStream> LoadAsync(IAggregateRootId aggregateId)
        {
            var es = new EventStream(Storage.Where(x => x.AggregateRootId.SequenceEqual(aggregateId.RawId)).ToList());
            return Task.FromResult(es);
        }
    }
}
