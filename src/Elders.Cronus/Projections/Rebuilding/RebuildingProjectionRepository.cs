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

        const ushort _batchSize = 1000;
        public async Task SaveAggregateCommitsAsync(IEnumerable<IndexRecord> indexRecords, RebuildProjection_JobData jobData)
        {
            List<Func<Task<string>>> indexingTasks = new List<Func<Task<string>>>(_batchSize);

            ushort currentSize = 0;
            foreach (IndexRecord record in indexRecords)
            {
                try
                {
                    currentSize++;

                    if (currentSize % _batchSize == 0)
                    {
                        await progressTracker.CompleteActionWithProgressSignalAsync(indexingTasks).ConfigureAwait(false);
                        indexingTasks.Clear();

                        if (jobData.IsCanceled)
                            return;
                    }
                    else
                    {
                        indexingTasks.Add(() => index.IndexAsync(record, jobData.Version));
                    }
                }
                catch (Exception ex) when (logger.WarnException(ex, () => $"Index record was skipped when rebuilding {jobData.Version.ProjectionName}.")) { }
            }
        }
    }
}
