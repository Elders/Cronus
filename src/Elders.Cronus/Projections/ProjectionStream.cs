using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elders.Cronus.Projections.Snapshotting;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections
{
    internal sealed class ProjectionStream
    {
        private readonly ILogger logger = CronusLogger.CreateLogger(typeof(ProjectionStream));

        private static readonly List<ProjectionCommit> _empty = new List<ProjectionCommit>();

        private readonly IBlobId projectionId;
        private readonly Func<ISnapshot> getSnapshot;
        private ISnapshot snapshot;

        ProjectionStream()
        {
            this.Commits = _empty;
            this.snapshot = new NoSnapshot(null, null);
        }

        public ProjectionStream(IBlobId projectionId, List<ProjectionCommit> commits, Func<ISnapshot> getSnapshot)
        {
            if (ReferenceEquals(null, projectionId)) throw new ArgumentException(nameof(projectionId));
            if (ReferenceEquals(null, commits)) throw new ArgumentException(nameof(commits));
            if (ReferenceEquals(null, getSnapshot)) throw new ArgumentException(nameof(getSnapshot));

            this.projectionId = projectionId;
            this.Commits = commits;
            this.getSnapshot = getSnapshot;
        }

        public List<ProjectionCommit> Commits { get; private set; }

        public IProjectionDefinition RestoreFromHistory(Type projectionType)
        {
            if (Commits.Count <= 0 && ReferenceEquals(null, GetSnapshot().State)) return null;

            IProjectionDefinition projection = (IProjectionDefinition)FastActivator.CreateInstance(projectionType, true);
            return RestoreFromHistoryMamamia(projection);
        }

        public T RestoreFromHistory<T>() where T : IProjectionDefinition
        {
            if (Commits.Count <= 0 && ReferenceEquals(null, GetSnapshot().State)) return default(T);

            T projection = (T)FastActivator.CreateInstance(typeof(T), true);
            return RestoreFromHistoryMamamia<T>(projection);
        }

        ISnapshot GetSnapshot()
        {
            if (ReferenceEquals(null, snapshot))
                snapshot = getSnapshot();

            return snapshot;
        }

        T RestoreFromHistoryMamamia<T>(T projection) where T : IProjectionDefinition
        {
            ISnapshot localSnapshot = GetSnapshot();
            projection.InitializeState(projectionId, localSnapshot.State);

            logger.Debug(() =>
            {
                var markers = Commits.Select(x => x.SnapshotMarker).DefaultIfEmpty(localSnapshot.Revision).ToList();
                var message =
                    $"Restoring projection `{typeof(T).Name}` from history...{Environment.NewLine}" +
                    $"ProjectionId: {Encoding.UTF8.GetString(projection.Id.RawId)}||{Convert.ToBase64String(projection.Id.RawId)}{Environment.NewLine}" +
                    $"Snapshot revision: {localSnapshot.Revision}{Environment.NewLine}" +
                    $"MIN/MAX snapshot marker: {markers.Min()}/{markers.Max()}{Environment.NewLine}" +
                    $"Projection commits after snapshot: {Commits.Count}";

                return message;
            });

            var groupedBySnapshotMarker = Commits.GroupBy(x => x.SnapshotMarker).OrderBy(x => x.Key);
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

            return projection;
        }

        private readonly static ProjectionStream _emptyProjectionStream = new ProjectionStream();

        public static ProjectionStream Empty()
        {
            return _emptyProjectionStream;
        }
    }
}
