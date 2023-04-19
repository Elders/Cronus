using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;

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


        public Task AppendAsync(AggregateEventRaw aggregateCommitRaw)
        {
            return Task.FromException(new System.NotImplementedException());
        }
        public Task<EventStream> LoadAsync(IBlobId aggregateId)
        {
            var es = new EventStream(Storage.Where(x => x.AggregateRootId.SequenceEqual(aggregateId.RawId)).ToList());
            return Task.FromResult(es);
        }

        public Task<bool> DeleteAsync(AggregateEventRaw eventRaw)
        {
            throw new System.NotImplementedException();
        }

        public Task<EventStream> LoadAsync(IBlobId aggregateId, int afterRevision)
        {
            var commits = Storage
                .Where(x => x.AggregateRootId.SequenceEqual(aggregateId.RawId))
                .Where(x => x.Revision > afterRevision)
                .ToList();

            var es = new EventStream(commits);
            return Task.FromResult(es);
        }
    }
}
