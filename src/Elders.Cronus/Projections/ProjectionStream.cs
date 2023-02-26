using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        internal ProjectionStream()
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

        public Task<IProjectionDefinition> RestoreFromHistoryAsync(Type projectionType)
        {
            if (Commits.Count <= 0 && ReferenceEquals(null, GetSnapshot().State)) return Task.FromResult(default(IProjectionDefinition));

            IProjectionDefinition projection = (IProjectionDefinition)FastActivator.CreateInstance(projectionType, true);
            return RestoreFromHistoryMamamiaAsync(projection);
        }

        public Task<T> RestoreFromHistoryAsync<T>() where T : IProjectionDefinition
        {
            if (Commits.Count <= 0 && ReferenceEquals(null, GetSnapshot().State)) return Task.FromResult(default(T));

            T projection = (T)FastActivator.CreateInstance(typeof(T), true);
            return RestoreFromHistoryMamamiaAsync<T>(projection);
        }

        public ISnapshot GetSnapshot()
        {
            if (ReferenceEquals(null, snapshot))
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

        public ProjectionStreamDto ToDto()
        {
            return new ProjectionStreamDto(GetSnapshot(), Commits);
        }
    }

    [DataContract(Name = "73a78656-c298-4cd2-ad78-f0eb5b866ad0")]
    public class ProjectionStreamDto
    {
        public ProjectionStreamDto() { }
        public ProjectionStreamDto(ISnapshot snapshot, List<ProjectionCommit> commits)
        {
            Snapshot = snapshot;
            Commits = commits;
        }

        [DataMember(Order = 1)]
        public ISnapshot Snapshot { get; private set; }

        [DataMember(Order = 2)]
        public List<ProjectionCommit> Commits { get; private set; }
    }
}
