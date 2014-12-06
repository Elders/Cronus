using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore
{
    public class EventStream
    {
        SortedList<int, AggregateCommit> eventStream;

        public EventStream(IList<AggregateCommit> aggregateCommits)
        {
            eventStream = new SortedList<int, AggregateCommit>(aggregateCommits.ToDictionary(x => x.Revision), Comparer<int>.Default);
        }

        public T RestoreFromHistory<T>() where T : ICanRestoreStateFromEvents<IAggregateRootState>
        {
            var ar = (T)FastActivator.CreateInstance(typeof(T), true);
            var events = eventStream.Values.SelectMany(x => x.Events).ToList();
            int currentRevision = eventStream.Last().Key;
            ar.ReplayEvents(events, currentRevision);
            return ar;
        }
    }
}