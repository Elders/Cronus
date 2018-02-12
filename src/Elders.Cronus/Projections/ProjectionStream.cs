using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elders.Cronus.Logging;
using Elders.Cronus.Projections.Snapshotting;

namespace Elders.Cronus.Projections
{
    public class ProjectionStream
    {
        static ILog log = LogProvider.GetLogger(typeof(ProjectionStream));

        private readonly IBlobId projectionId;
        IList<ProjectionCommit> commits;
        readonly ISnapshot snapshot;

        public ProjectionStream(IBlobId projectionId, IList<ProjectionCommit> commits, ISnapshot snapshot)
        {
            if (ReferenceEquals(null, projectionId) == true) throw new ArgumentException(nameof(projectionId));
            if (ReferenceEquals(null, commits) == true) throw new ArgumentException(nameof(commits));
            if (ReferenceEquals(null, snapshot) == true) throw new ArgumentException(nameof(snapshot));

            this.projectionId = projectionId;
            this.commits = commits;
            this.snapshot = snapshot;
        }

        public IEnumerable<ProjectionCommit> Commits { get { return commits.ToList().AsReadOnly(); } }

        public IProjectionGetResult<IProjectionDefinition> RestoreFromHistory(Type projectionType)
        {
            if (commits.Count <= 0 && ReferenceEquals(null, snapshot.State)) return ProjectionGetResult<IProjectionDefinition>.NoResult;

            IProjectionDefinition projection = (IProjectionDefinition)FastActivator.CreateInstance(projectionType, true);
            return RestoreFromHistoryMamamia(projection);
        }

        public IProjectionGetResult<T> RestoreFromHistory<T>() where T : IProjectionDefinition
        {
            if (commits.Count <= 0 && ReferenceEquals(null, snapshot.State)) return ProjectionGetResult<T>.NoResult;

            T projection = (T)Activator.CreateInstance(typeof(T), true);
            return RestoreFromHistoryMamamia<T>(projection);
        }

        IProjectionGetResult<T> RestoreFromHistoryMamamia<T>(T projection) where T : IProjectionDefinition
        {
            projection.InitializeState(projectionId, snapshot.State);

            log.Debug(() => $"Restoring projection `{typeof(T).Name}` from history...{Environment.NewLine}" +
                $"ProjectionId: {Encoding.UTF8.GetString(projection.Id.RawId)}||{Convert.ToBase64String(projection.Id.RawId)}{Environment.NewLine}" +
                $"Snapshot revision: {snapshot.Revision}{Environment.NewLine}" +
                $"MIN/MAX snapshot marker: {commits.Select(x => x.SnapshotMarker).DefaultIfEmpty(snapshot.Revision).Min()}/{commits.Select(x => x.SnapshotMarker).DefaultIfEmpty(snapshot.Revision).Max()}{Environment.NewLine}" +
                $"Projection commits after snapshot: {commits.Count}");

            var groupedBySnapshotMarker = commits.GroupBy(x => x.SnapshotMarker).OrderBy(x => x.Key);
            foreach (var snapshotGroup in groupedBySnapshotMarker)
            {
                var eventsByAggregate = snapshotGroup.GroupBy(x => x.EventOrigin.AggregateRootId);

                foreach (var aggregateGroup in eventsByAggregate)
                {
                    var events = aggregateGroup
                        .OrderBy(x => x.EventOrigin.AggregateRevision)
                        .ThenBy(x => x.EventOrigin.AggregateEventPosition)
                        .Select(x => x.Event);

                    projection.ReplayEvents(events);
                }
            }

            return new ProjectionGetResult<T>(projection);
        }
    }
}
