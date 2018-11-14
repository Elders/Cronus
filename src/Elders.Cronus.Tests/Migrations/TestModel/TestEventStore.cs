using Elders.Cronus.EventStore;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Migration.Middleware.Tests.TestModel
{
    public class TestEventStore : IEventStore
    {
        public IList<AggregateCommit> Storage { get; private set; }

        public TestEventStore()
        {
            Storage = new List<AggregateCommit>();
        }

        public void Append(AggregateCommit aggregateCommit)
        {
            Storage.Add(aggregateCommit);
        }

        public EventStream Load(IAggregateRootId aggregateId)
        {
            var es = new EventStream(Storage.Where(x => x.AggregateRootId.SequenceEqual(aggregateId.RawId)).ToList());
            return es;
        }
    }
}
