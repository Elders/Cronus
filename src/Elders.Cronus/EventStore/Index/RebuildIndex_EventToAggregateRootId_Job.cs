using Elders.Cronus.Cluster.Job;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    public class RebuildIndex_EventToAggregateRootId_Job : CronusJob<RebuildIndexJobData>
    {
        private readonly IEventStorePlayer eventStorePlayer;
        private readonly EventToAggregateRootId index;

        public RebuildIndex_EventToAggregateRootId_Job(IEventStorePlayer eventStorePlayer, EventToAggregateRootId index)
        {
            this.eventStorePlayer = eventStorePlayer;
            this.index = index;
        }

        public override string Name => typeof(EventToAggregateRootId).GetContractId();

        public override RebuildIndexJobData BuildInitialData() => new RebuildIndexJobData();

        protected override async Task<JobExecutionStatus> RunJob(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            bool hasMoreRecords = true;
            while (hasMoreRecords)
            {
                var result = eventStorePlayer.LoadAggregateCommits(Data.PaginationToken);
                foreach (var aggregateCommit in result.Commits)
                {
                    index.Index(aggregateCommit);

                    Data.PaginationToken = result.PaginationToken;
                    Data = await cluster.PingAsync(Data).ConfigureAwait(false);
                }

                Data.IsCompleted = true;
                Data = await cluster.PingAsync(Data).ConfigureAwait(false);

                hasMoreRecords = result.Commits.Count > 0;
            }
            //indexStatus.Save(StateId, IndexStatus.Present);

            return JobExecutionStatus.Completed;
        }
    }

    public class RebuildIndexJobData
    {
        public bool IsCompleted { get; set; } = false;

        public string PaginationToken { get; set; } = string.Empty;

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
