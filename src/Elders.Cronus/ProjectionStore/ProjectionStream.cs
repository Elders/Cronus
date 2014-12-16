using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.ProjectionStore
{
    public class ProjectionStream
    {
        SortedList<int, ProjectionCommit> projectionStream;

        public ProjectionStream(IList<ProjectionCommit> projectionCommits)
        {
            projectionStream = new SortedList<int, ProjectionCommit>(projectionCommits.ToDictionary(x => x.AggregateRootRevision), Comparer<int>.Default);
        }

        public T RestoreFromHistory<T>() where T : ICanRestoreStateFromEvents<IAggregateRootState>
        {
            var ar = (T)FastActivator.CreateInstance(typeof(T), true);
            //var events = projectionStream.Values.SelectMany(x => x.Events).ToList();
            //int currentRevision = projectionStream.Last().Key;
            //ar.ReplayEvents(events, currentRevision);
            return ar;
        }
    }
}