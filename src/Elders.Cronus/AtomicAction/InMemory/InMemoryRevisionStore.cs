using System.Collections.Concurrent;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Userfull;

namespace Elders.Cronus.AtomicAction.InMemory
{
    public class InMemoryRevisionStore : IRevisionStore
    {
        static ConcurrentDictionary<IAggregateRootId, AtomicInteger> aggregateRevisions = new ConcurrentDictionary<IAggregateRootId, AtomicInteger>();

        public Result<bool> HasRevision(IAggregateRootId aggregateRootId)
        {
            var hasRevision = aggregateRevisions.ContainsKey(aggregateRootId);
            return new Result<bool>(hasRevision);
        }

        public Result<int> GetRevision(IAggregateRootId aggregateRootId)
        {
            AtomicInteger revision;
            if (aggregateRevisions.TryGetValue(aggregateRootId, out revision) == false)
                return new Result<int>(default(int)).WithError($"Unable to find a revision for '{aggregateRootId}'");

            return new Result<int>(revision.Value);
        }

        public Result<bool> SaveRevision(IAggregateRootId aggregateRootId, int revision)
        {
            var atomicRevision = new AtomicInteger(revision);
            var isSuccessfulStore = aggregateRevisions.TryAdd(aggregateRootId, atomicRevision);
            var result = new Result<bool>(isSuccessfulStore);

            if (isSuccessfulStore == false)
                result.WithError($"Unable to store a revision '{revision}' for '{aggregateRootId}'");

            return result;
        }

        public void Dispose() { }
    }
}