using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections
{
    internal sealed class ProjectionStream
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(ProjectionStream));

        private static readonly List<ProjectionCommitPreview> _empty = new List<ProjectionCommitPreview>();

        private readonly IBlobId projectionId;

        ProjectionStream()
        {
            this.Commits = _empty;
        }

        public ProjectionStream(IBlobId projectionId, List<ProjectionCommitPreview> commits)
        {
            if (projectionId is null) throw new ArgumentException(nameof(projectionId));
            if (commits is null) throw new ArgumentException(nameof(commits));

            this.projectionId = projectionId;
            this.Commits = commits;
        }

        public List<ProjectionCommitPreview> Commits { get; private set; }

        public Task<IProjectionDefinition> RestoreFromHistoryAsync(Type projectionType)
        {
            if (Commits.Count <= 0)
                return Task.FromResult(default(IProjectionDefinition));

            IProjectionDefinition projection = (IProjectionDefinition)FastActivator.CreateInstance(projectionType, true);
            return RestoreFromHistoryMamamiaAsync(projection);
        }

        public Task<T> RestoreFromHistoryAsync<T>() where T : IProjectionDefinition
        {
            if (Commits.Count <= 0)
                return Task.FromResult(default(T));

            T projection = (T)FastActivator.CreateInstance(typeof(T), true);
            return RestoreFromHistoryMamamiaAsync<T>(projection);
        }

        async Task<T> RestoreFromHistoryMamamiaAsync<T>(T projection) where T : IProjectionDefinition
        {
            IEnumerable<IEvent> events = Commits
                .Select(commit => commit.Event)
                .OrderBy(@event => @event.Timestamp);

            foreach (IEvent @event in events)
            {
                await projection.ReplayEventAsync(@event).ConfigureAwait(false);
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
