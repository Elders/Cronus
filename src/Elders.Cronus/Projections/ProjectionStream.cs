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

        Func<ISnapshot> getSnapshot;
        ISnapshot snapshot;

        ProjectionStream()
        {
            this.commits = Enumerable.Empty<ProjectionCommit>().ToList();
            this.snapshot = new NoSnapshot(null, null);
        }

        public ProjectionStream(IBlobId projectionId, IList<ProjectionCommit> commits, Func<ISnapshot> getSnapshot)
        {
            if (ReferenceEquals(null, projectionId)) throw new ArgumentException(nameof(projectionId));
            if (ReferenceEquals(null, commits)) throw new ArgumentException(nameof(commits));
            if (ReferenceEquals(null, getSnapshot)) throw new ArgumentException(nameof(getSnapshot));

            this.projectionId = projectionId;
            this.commits = commits;
            this.getSnapshot = getSnapshot;
        }

        public IEnumerable<ProjectionCommit> Commits { get { return commits.ToList().AsReadOnly(); } }

        public IProjectionGetResult<IProjectionDefinition> RestoreFromHistory(Type projectionType)
        {
            if (commits.Count <= 0 && ReferenceEquals(null, GetSnapshot().State)) return ProjectionGetResult<IProjectionDefinition>.NoResult;

            IProjectionDefinition projection = (IProjectionDefinition)FastActivator.CreateInstance(projectionType, true);
            return RestoreFromHistoryMamamia(projection);
        }

        public IProjectionGetResult<T> RestoreFromHistory<T>() where T : IProjectionDefinition
        {
            if (commits.Count <= 0 && ReferenceEquals(null, GetSnapshot().State)) return ProjectionGetResult<T>.NoResult;

            T projection = (T)Activator.CreateInstance(typeof(T), true);
            return RestoreFromHistoryMamamia<T>(projection);
        }

        ISnapshot GetSnapshot()
        {
            if (ReferenceEquals(null, snapshot))
                snapshot = getSnapshot();

            return snapshot;
        }

        IProjectionGetResult<T> RestoreFromHistoryMamamia<T>(T projection) where T : IProjectionDefinition
        {
            ISnapshot localSnapshot = GetSnapshot();
            projection.InitializeState(projectionId, localSnapshot.State);

            log.Debug(() =>
            {
                var markers = commits.Select(x => x.SnapshotMarker).DefaultIfEmpty(localSnapshot.Revision).ToList();
                var message =
                    $"Restoring projection `{typeof(T).Name}` from history...{Environment.NewLine}" +
                    $"ProjectionId: {Encoding.UTF8.GetString(projection.Id.RawId)}||{Convert.ToBase64String(projection.Id.RawId)}{Environment.NewLine}" +
                    $"Snapshot revision: {localSnapshot.Revision}{Environment.NewLine}" +
                    $"MIN/MAX snapshot marker: {markers.Min()}/{markers.Max()}{Environment.NewLine}" +
                    $"Projection commits after snapshot: {commits.Count}";

                return message;
            });

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

        public static ProjectionStream Empty()
        {
            return new ProjectionStream();
        }
    }
}
