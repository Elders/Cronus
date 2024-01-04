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

        public async Task<T> RestoreFromHistoryAsync<T>(T projection) where T : IProjectionDefinition
        {
            if (Commits.Count <= 0)
                return default(T);

            IEnumerable<IEvent> events = Commits
                .Select(commit => commit.Event)
                .OrderBy(@event => @event.Timestamp);

            projection.InitializeState(projectionId, null);
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
