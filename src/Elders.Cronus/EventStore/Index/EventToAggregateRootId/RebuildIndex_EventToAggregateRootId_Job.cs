using Elders.Cronus.Cluster.Job;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    public class RebuildIndex_EventToAggregateRootId_Job : CronusJob<RebuildIndex_JobData>
    {
        private readonly IEventStorePlayer eventStorePlayer;
        private readonly IEventStoreJobIndex index;

        public RebuildIndex_EventToAggregateRootId_Job(IEventStorePlayer eventStorePlayer, IEventStoreJobIndex index, ILogger<RebuildIndex_EventToAggregateRootId_Job> logger) : base(logger)
        {
            this.eventStorePlayer = eventStorePlayer;
            this.index = index;
        }

        public override string Name { get; set; } = typeof(EventToAggregateRootId).GetContractId();

        protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            bool hasMoreRecords = true;
            while (hasMoreRecords && Data.IsCompleted == false)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.Info(() => $"The job has been cancelled.");
                    return JobExecutionStatus.Running;
                }

                var result = await eventStorePlayer.LoadAggregateCommitsAsync(Data.PaginationToken).ConfigureAwait(false);

                logger.Info(() => $"Loaded aggregate commits count {result.Commits.Count} using pagination token {result.PaginationToken}");

                List<Task> tasks = new List<Task>();
                foreach (var aggregateCommit in result.Commits)
                {
                    Task task = index.IndexAsync(aggregateCommit);
                    tasks.Add(task);

                    if (tasks.Count > 100)
                    {
                        Task finished = await Task.WhenAny(tasks).ConfigureAwait(false);
                        tasks.Remove(finished);
                    }
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                Data.PaginationToken = result.PaginationToken;
                Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

                hasMoreRecords = result.HasMoreResults;
            }

            Data.IsCompleted = true;
            Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

            logger.Info(() => $"The job has been completed.");

            return JobExecutionStatus.Completed;
        }
    }
}
