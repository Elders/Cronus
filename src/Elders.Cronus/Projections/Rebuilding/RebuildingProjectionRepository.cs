using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus.Projections.Rebuilding
{
    public class RebuildingProjectionRepository : IRebuildingProjectionRepository
    {
        private readonly ProgressTracker progressTracker;
        private readonly ProjectionIndex index;
        private readonly ILogger logger;

        public RebuildingProjectionRepository(ProjectionIndex index, ProgressTracker progressTracker, ILogger<RebuildingProjectionRepository> logger)
        {
            this.progressTracker = progressTracker;
            this.index = index;
            this.logger = logger;
        }

        public async Task SaveAggregateCommitsAsync(IEnumerable<IndexRecord> indexRecords, RebuildProjection_JobData jobData)
        {
            // The trick with the task should be one level above. This method should be responsible for handling single record.
            List<Task> tasks = new List<Task>();
            foreach (IndexRecord record in indexRecords)
            {
                if (jobData.IsCanceled)
                    return;

                Task task = ExecuteAndTrack(record, jobData.Version);
                tasks.Add(task);

                if (tasks.Count > 100)
                {
                    Task finished = await Task.WhenAny(tasks).ConfigureAwait(false);
                    tasks.Remove(finished);
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task ExecuteAndTrack(IndexRecord record, ProjectionVersion projectionVersion)
        {
            try
            {
                string executionId = await index.IndexAsync(record, projectionVersion).ConfigureAwait(false);
                progressTracker.TrackAndNotify(executionId);
            }
            catch (Exception ex) when (logger.WarnException(ex, () => $"Index record was skipped when rebuilding {projectionVersion.ProjectionName}.")) { }
        }
    }
}
