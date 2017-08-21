using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.EventStore.InMemory
{
    public class InMemoryEventStoreStorage : IDisposable
    {
        private ConcurrentQueue<AggregateCommit> eventsForReplay;
        private ConcurrentDictionary<string, ConcurrentQueue<AggregateCommit>> eventsStreams;

        public InMemoryEventStoreStorage()
        {
            eventsStreams = new ConcurrentDictionary<string, ConcurrentQueue<AggregateCommit>>();
            eventsForReplay = new ConcurrentQueue<AggregateCommit>();
        }


        /// <summary>
        /// Gets the ordered events.
        /// </summary>
        /// <returns></returns>
        internal protected IEnumerable<AggregateCommit> GetOrderedEvents()
        {
            while (!eventsForReplay.IsEmpty)
            {
                AggregateCommit commit;
                if (eventsForReplay.TryDequeue(out commit))
                {
                    yield return commit;
                }
            }
        }


        /// <summary>
        /// Flushes the specified aggregate commit.
        /// </summary>
        /// <param name="aggregateCommit">The aggregate commit.</param>
        internal protected void Flush(AggregateCommit aggregateCommit)
        {
            var idHash = Convert.ToBase64String(aggregateCommit.AggregateRootId);
            if (!eventsStreams.ContainsKey(idHash))
            {
                eventsStreams.TryAdd(idHash, new ConcurrentQueue<AggregateCommit>());
            }

            var evnts = eventsStreams[idHash];

            evnts.Enqueue(aggregateCommit);
            eventsForReplay.Enqueue(aggregateCommit);
        }

        internal protected List<AggregateCommit> Seek(IAggregateRootId aggregateId)
        {
            var idHash = Convert.ToBase64String(aggregateId.RawId);
            ConcurrentQueue<AggregateCommit> commits;
            if (eventsStreams.TryGetValue(idHash, out commits))
                return commits.ToList();
            else
                return new List<AggregateCommit>();
        }

        public void Dispose()
        {
            if (eventsStreams != null)
                eventsStreams = null;

            if (eventsForReplay != null)
                eventsForReplay = null;
        }
    }
}
