using Elders.Cronus.Cluster.Job;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly ILogger<RebuildIndex_EventToAggregateRootId_Job> logger;

        public RebuildIndex_EventToAggregateRootId_Job(IEventStorePlayer eventStorePlayer, EventToAggregateRootId index, ILogger<RebuildIndex_EventToAggregateRootId_Job> logger)
        {
            this.eventStorePlayer = eventStorePlayer;
            this.index = index;
            this.logger = logger;
        }

        public override string Name { get; set; } = typeof(EventToAggregateRootId).GetContractId();

        protected override RebuildIndex_JobData BuildInitialData() => new RebuildIndex_JobData();

        protected override async Task<JobExecutionStatus> RunJob(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.Info(() => $"The job {Name} was cancelled before it got started.");
                return JobExecutionStatus.Running;
            }

            bool hasMoreRecords = true;
            while (hasMoreRecords && Data.IsCompleted == false)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.Info(() => $"The job {Name} was cancelled.");
                    return JobExecutionStatus.Running;
                }

                var result = eventStorePlayer.LoadAggregateCommits(Data.PaginationToken);
                logger.Info(() => $"Loaded aggregate commits count ${result.Commits.Count} using pagination token {result.PaginationToken}");
                foreach (var aggregateCommit in result.Commits)
                {
                    index.Index(aggregateCommit);
                }

                Data.PaginationToken = result.PaginationToken;
                Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

                hasMoreRecords = result.Commits.Any();
            }

            Data.IsCompleted = true;
            Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

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
        private readonly IJobNameBuilder jobNameBuilder;

        public RebuildIndex_EventToAggregateRootId_JobFactory(RebuildIndex_EventToAggregateRootId_Job job, IJobNameBuilder jobNameBuilder)
        {
            this.job = job;
            this.jobNameBuilder = jobNameBuilder;
        }

        public RebuildIndex_EventToAggregateRootId_Job CreateJob(VersionRequestTimebox timebox)
        {
            job.Name = jobNameBuilder.GetJobName(job.Name);
            job.SetTimeBox(timebox);

            return job;
        }

        public string GetJobName()
        {
            return jobNameBuilder.GetJobName(job.Name);
        }
    }

    public interface IJobNameBuilder
    {
        string GetJobName(string defaultName);
    }

    public class DefaultJobNameBuilder : IJobNameBuilder
    {
        private readonly BoundedContext boundedContext;
        private readonly CronusContext context;

        public DefaultJobNameBuilder(IOptions<BoundedContext> boundedContext, CronusContext context)
        {
            this.boundedContext = boundedContext.Value;
            this.context = context;
        }

        public string GetJobName(string defaultName)
        {
            return $"urn:{boundedContext.Name}:{context.Tenant}:{defaultName}";
        }
    }


    public class RebuildIndex_JobData
    {
        public bool IsCompleted { get; set; } = false;

        public string PaginationToken { get; set; }

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
