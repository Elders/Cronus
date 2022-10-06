using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.EventStore;

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
            foreach (IndexRecord record in indexRecords)
            {
                try
                {
                    await progressTracker.CompleteActionWithProgressSignalAsync(() => index.IndexAsync(record, jobData.Version)).ConfigureAwait(false);

                    if (jobData.IsCanceled)
                        return;
                }
                catch (Exception ex) when (logger.WarnException(ex, () => $"Index record was skipped when rebuilding {jobData.Version.ProjectionName}.")) { }
            }
        }
    }
}
