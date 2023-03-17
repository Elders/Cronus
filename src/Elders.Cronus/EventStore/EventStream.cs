using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elders.Cronus.EventStore
{
    public class AggregateStream
    {
        List<AggregateCommitRaw> commits;

        public AggregateStream(IEnumerable<AggregateEventRaw> events)
        {
            commits = new List<AggregateCommitRaw>();
            IEnumerable<IGrouping<int, AggregateEventRaw>> byRevision = events.GroupBy(x => x.Revision);
            foreach (IGrouping<int, AggregateEventRaw> revisionEvents in byRevision)
            {
                AggregateCommitRaw commit = new AggregateCommitRaw(revisionEvents);
                commits.Add(commit);
            }
        }
    }

    public class EventStream
    {
        IList<AggregateCommit> aggregateCommits;

        public EventStream(IList<AggregateCommit> aggregateCommits)
        {
            this.aggregateCommits = aggregateCommits;
            EventsCount = aggregateCommits.SelectMany(x => x.Events).Count();
        }

        public IEnumerable<AggregateCommit> Commits { get { return aggregateCommits; } }
        public int Count => aggregateCommits.Count;
        public int EventsCount { get; }

        public bool TryRestoreFromSnapshot<T>(object state, int snapshotRevision, out T aggregateRoot)
            where T : IAmEventSourced
        {
            aggregateRoot = default;

            if (state is null)
                return false;

            var events = aggregateCommits.SelectMany(x => x.Events);

            int currentRevision = aggregateCommits.Any() ? aggregateCommits.Last().Revision : snapshotRevision;
            aggregateRoot = (T)FastActivator.CreateInstance(typeof(T), true);
            aggregateRoot.ReplayEvents(events.ToList(), currentRevision, (IAggregateRootState)state);
            return true;
        }

        public bool TryRestoreFromHistory<T>(out T aggregateRoot) where T : IAmEventSourced
        {
            //http://www.datastax.com/dev/blog/client-side-improvements-in-cassandra-2-0
            aggregateRoot = default(T);
            var events = aggregateCommits.SelectMany(x => x.Events);
            if (events.Any())
            {
                int currentRevision = aggregateCommits.Last().Revision;
                aggregateRoot = (T)FastActivator.CreateInstance(typeof(T), true);
                aggregateRoot.ReplayEvents(events.ToList(), currentRevision);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryRestoreFromHistory(int revision, Type aggregateRootType, out object aggregateRoot)
        {
            //http://www.datastax.com/dev/blog/client-side-improvements-in-cassandra-2-0
            aggregateRoot = default;
            var events = aggregateCommits.Where(x => x.Revision <= revision).SelectMany(x => x.Events);
            if (events.Any())
            {
                int currentRevision = aggregateCommits.Last().Revision;
                aggregateRoot = FastActivator.CreateInstance(aggregateRootType, true);
                ((IAmEventSourced)aggregateRoot).ReplayEvents(events.ToList(), currentRevision);
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
            string base64AggregateRootId = Convert.ToBase64String(firstCommit.AggregateRootId);
            string aggregateName = Encoding.UTF8.GetString(firstCommit.AggregateRootId).Split('@')[0];

            performanceCriticalOutputBuilder.AppendLine("Aggregate Info");
            performanceCriticalOutputBuilder.AppendLine("==============");
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
