using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore
{
    public class EventStream
    {
        internal IList<AggregateCommit> aggregateCommits;

        public EventStream(IList<AggregateCommit> aggregateCommits)
        {
            this.aggregateCommits = aggregateCommits;
        }

        public T RestoreFromHistory<T>() where T : IAmEventSourced
        {
            //http://www.datastax.com/dev/blog/client-side-improvements-in-cassandra-2-0
            var ar = (T)FastActivator.CreateInstance(typeof(T), true);
            var events = aggregateCommits.SelectMany(x => x.Events).ToList();
            int currentRevision = aggregateCommits.Last().Revision;

            ar.ReplayEvents(events, currentRevision);
            return ar;
        }

        public bool TryRestoreFromHistory<T>(out T aggregateRoot) where T : IAmEventSourced
        {
            aggregateRoot = default(T);
            var events = aggregateCommits.SelectMany(x => x.Events).ToList();
            if (events.Count > 0)
            {
                int currentRevision = aggregateCommits.Last().Revision;
                aggregateRoot = (T)FastActivator.CreateInstance(typeof(T), true);
                aggregateRoot.ReplayEvents(events, currentRevision);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            var performanceCriticalOutputBuilder = new StringBuilder();
            foreach (var commit in aggregateCommits)
            {
                foreach (var @event in commit.Events)
                {
                    performanceCriticalOutputBuilder.AppendLine($"\t-{@event}");
                }
            }
            return performanceCriticalOutputBuilder.ToString();
        }
    }
}
