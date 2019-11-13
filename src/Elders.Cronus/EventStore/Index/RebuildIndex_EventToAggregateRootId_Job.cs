using Elders.Cronus.Cluster.Job;
using Elders.Cronus.Projections.Versioning;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    public class RebuildIndex_EventToAggregateRootId_Job : CronusJob<RebuildIndex_JobData>
    {
        private readonly IEventStorePlayer eventStorePlayer;
        private readonly EventToAggregateRootId index;

        public RebuildIndex_EventToAggregateRootId_Job(IEventStorePlayer eventStorePlayer, EventToAggregateRootId index)
        {
            this.eventStorePlayer = eventStorePlayer;
            this.index = index;
        }

        public override string Name => typeof(EventToAggregateRootId).GetContractId();

        protected override RebuildIndex_JobData BuildInitialData() => new RebuildIndex_JobData();

        protected override async Task<JobExecutionStatus> RunJob(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            bool hasMoreRecords = true;
            while (hasMoreRecords && Data.IsCompleted == false)
            {
                var result = eventStorePlayer.LoadAggregateCommits(Data.PaginationToken);
                foreach (var aggregateCommit in result.Commits)
                {
                    index.Index(aggregateCommit);
                }

                Data.PaginationToken = result.PaginationToken;
                Data = await cluster.PingAsync(Data).ConfigureAwait(false);

                hasMoreRecords = result.Commits.Any();
            }

            Data.IsCompleted = true;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            return JobExecutionStatus.Completed;
        }

        public void SetTimeBox(VersionRequestTimebox timebox)
        {
            var dataOverride = BuildInitialData();
            dataOverride.Timestamp = timebox.RebuildStartAt;

            OverrideData(fromCluster => Override(fromCluster, dataOverride));
        }

        private RebuildIndex_JobData Override(RebuildIndex_JobData fromCluster, RebuildIndex_JobData dataOverride)
        {
            if (fromCluster.IsCompleted && fromCluster.Timestamp < dataOverride.Timestamp)
                return dataOverride;
            else
                return fromCluster;
        }
    }

    public class RebuildIndex_EventToAggregateRootId_JobFactory
    {
        private readonly RebuildIndex_EventToAggregateRootId_Job job;

        public RebuildIndex_EventToAggregateRootId_JobFactory(RebuildIndex_EventToAggregateRootId_Job job)
        {
            this.job = job;
        }

        public RebuildIndex_EventToAggregateRootId_Job CreateJob(VersionRequestTimebox timebox)
        {
            job.SetTimeBox(timebox);

            return job;
        }
    }

    public class RebuildIndex_JobData
    {
        public bool IsCompleted { get; set; } = false;

        public string PaginationToken { get; set; } = string.Empty;

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
