using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.Projections.Snapshotting;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections
{
    internal sealed class ProjectionStream
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(ProjectionStream));

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
            if (projectionId is null) throw new ArgumentException(nameof(projectionId));
            if (commits is null) throw new ArgumentException(nameof(commits));
            if (getSnapshot is null) throw new ArgumentException(nameof(getSnapshot));

            this.projectionId = projectionId;
            this.Commits = commits;
            this.getSnapshot = getSnapshot;
        }

        public List<ProjectionCommit> Commits { get; private set; }

        public Task<IProjectionDefinition> RestoreFromHistoryAsync(Type projectionType)
        {
            if (Commits.Count <= 0 && GetSnapshot().State is null) return Task.FromResult(default(IProjectionDefinition));

            IProjectionDefinition projection = (IProjectionDefinition)FastActivator.CreateInstance(projectionType, true);
            return RestoreFromHistoryMamamiaAsync(projection);
        }

        public Task<T> RestoreFromHistoryAsync<T>() where T : IProjectionDefinition
        {
            if (Commits.Count <= 0 && GetSnapshot().State is null) return Task.FromResult(default(T));

            T projection = (T)FastActivator.CreateInstance(typeof(T), true);
            return RestoreFromHistoryMamamiaAsync<T>(projection);
        }

        ISnapshot GetSnapshot()
        {
            if (snapshot is null)
                snapshot = getSnapshot();

            return snapshot;
        }

        async Task<T> RestoreFromHistoryMamamiaAsync<T>(T projection) where T : IProjectionDefinition
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
                IEnumerable<IEvent> events = snapshotGroup
                    .OrderBy(x => x.EventOrigin.Timestamp)
                    .Select(x => x.Event);

                foreach (var @event in events)
                {
                    await projection.ReplayEventAsync(@event).ConfigureAwait(false);
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
