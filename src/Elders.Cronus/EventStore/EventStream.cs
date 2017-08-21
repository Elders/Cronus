using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elders.Cronus.EventStore
{
    public class EventStream
    {
        IList<AggregateCommit> aggregateCommits;

        public EventStream(IList<AggregateCommit> aggregateCommits)
        {
            this.aggregateCommits = aggregateCommits;
        }

        public IEnumerable<AggregateCommit> Commits { get { return aggregateCommits.ToList().AsReadOnly(); } }

        public bool TryRestoreFromHistory<T>(out T aggregateRoot) where T : IAmEventSourced
        {
            //http://www.datastax.com/dev/blog/client-side-improvements-in-cassandra-2-0
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
            AggregateCommit firstCommit = aggregateCommits.First();
            string boundedContext = firstCommit.BoundedContext;
            string base64AggregateRootId = Convert.ToBase64String(firstCommit.AggregateRootId);
            string aggregateName = Encoding.UTF8.GetString(firstCommit.AggregateRootId).Split('@')[0];

            performanceCriticalOutputBuilder.AppendLine("Aggregate Info");
            performanceCriticalOutputBuilder.AppendLine("==============");
            performanceCriticalOutputBuilder.AppendLine($"- Bounded Context: `{boundedContext}`");
            performanceCriticalOutputBuilder.AppendLine($"- Aggregate root ID (base64): `{base64AggregateRootId}`");
            performanceCriticalOutputBuilder.AppendLine($"- Aggregate name: `{aggregateName}`");
            performanceCriticalOutputBuilder.AppendLine();
            performanceCriticalOutputBuilder.AppendLine("## Commits");

            foreach (var commit in aggregateCommits)
            {

                performanceCriticalOutputBuilder.AppendLine();
                performanceCriticalOutputBuilder.AppendLine($"#### Revision {commit.Revision}");

                foreach (var @event in commit.Events)
                {
                    performanceCriticalOutputBuilder.AppendLine($"- {@event}");
                }
            }
            performanceCriticalOutputBuilder.AppendLine("-------------------------------------------------------------------------------------------------------------------");
            return performanceCriticalOutputBuilder.ToString();
        }
    }
}
