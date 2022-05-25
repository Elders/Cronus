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

        public RebuildIndex_EventToAggregateRootId_Job(IEventStorePlayer eventStorePlayer, EventToAggregateRootId index, ILogger<RebuildIndex_EventToAggregateRootId_Job> logger) : base(logger)
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

                LoadAggregateCommitsResult result = await eventStorePlayer.LoadAggregateCommitsAsync(Data.PaginationToken).ConfigureAwait(false);

                logger.Info(() => $"Loaded aggregate commits count ${result.Commits.Count} using pagination token {result.PaginationToken}");
                foreach (var aggregateCommit in result.Commits)
                {
                    await index.IndexAsync(aggregateCommit).ConfigureAwait(false);
                }

                Data.PaginationToken = result.PaginationToken;
                Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

                hasMoreRecords = result.Commits.Any();
            }

            Data.IsCompleted = true;
            Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

            logger.Info(() => $"The job has been completed.");

            return JobExecutionStatus.Completed;
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
            job.BuildInitialData(() => new RebuildIndex_JobData()
            {
                Timestamp = timebox.RequestStartAt
            });

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


    public class RebuildIndex_JobData : IJobData
    {
        public bool IsCompleted { get; set; } = false;

        public string PaginationToken { get; set; }

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }

    public interface IJobData
    {
        bool IsCompleted { get; set; }

        ///// <summary>
        ///// Indicates if the job data has been created locally. If the job data has been downloaded from the cluster the value will be false.
        ///// </summary>
        //bool IsLocal { get; set; }

        DateTimeOffset Timestamp { get; set; }
    }
}
