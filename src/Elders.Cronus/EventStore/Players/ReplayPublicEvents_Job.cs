using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Players
{
    public class ReplayPublicEvents_Job : CronusJob<ReplayPublicEvents_JobData>
    {
        private readonly IPublisher<IPublicEvent> publicEventPublisher;
        private readonly IEventStorePlayer eventStorePlayer;
        private readonly EventToAggregateRootId eventToAggregateIndex;

        public ReplayPublicEvents_Job(IPublisher<IPublicEvent> publicEventPublisher, IEventStorePlayer eventStorePlayer, EventToAggregateRootId eventToAggregateIndex, ILogger<ReplayPublicEvents_Job> logger) : base(logger)
        {
            this.publicEventPublisher = publicEventPublisher;
            this.eventStorePlayer = eventStorePlayer;
            this.eventToAggregateIndex = eventToAggregateIndex;
        }

        public override string Name { get; set; } = "c0e0f5fc-1f22-4022-96d0-bf02590951d6";

        protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            Dictionary<int, string> processedAggregates = new Dictionary<int, string>();


            string eventTypeId = Data.SourceEventTypeId;
            bool hasMoreRecords = true;
            while (hasMoreRecords && Data.IsCompleted == false)
            {
                string paginationToken = Data.EventTypePaging?.PaginationToken;
                LoadIndexRecordsResult indexRecordsResult = eventToAggregateIndex.EnumerateRecords(eventTypeId, paginationToken);
                IEnumerable<IndexRecord> indexRecords = indexRecordsResult.Records;
                Type publicEventType = typeof(IPublicEvent);
                ReplayOptions opt = new ReplayOptions()
                {
                    PaginationToken = paginationToken,
                    AggregateIds = indexRecordsResult.Records.Select(indexRecord => AggregateUrn.Parse(Convert.ToBase64String(indexRecord.AggregateRootId), Urn.Base64)),
                    ShouldSelect = commit =>
                    {
                        bool result = (from publicEvent in commit.PublicEvents
                                       let eventType = publicEvent.GetType()
                                       where publicEventType.IsAssignableFrom(eventType)
                                       where eventType.GetContractId().Equals(eventTypeId)
                                       select publicEvent)
                                       .Any();

                        return result;
                    },
                    After = Data.After,
                    Before = Data.Before
                };
                var hala = eventStorePlayer.LoadAggregateCommits(opt);

                foreach (AggregateCommit arCommit in hala.Commits)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        logger.Info(() => $"Job has been cancelled.");
                        return JobExecutionStatus.Running;
                    }

                    foreach (IPublicEvent publicEvent in arCommit.PublicEvents)
                    {
                        var headers = new Dictionary<string, string>()
                        {
                            {MessageHeader.RecipientBoundedContext, Data.RecipientBoundedContext},
                            {MessageHeader.RecipientHandlers, Data.RecipientHandlers}
                        };
                        publicEventPublisher.Publish(publicEvent, headers);
                    }
                }


                var progress = new ReplayPublicEvents_JobData.EventTypePagingProgress(eventTypeId, indexRecordsResult.PaginationToken, 0, 0);
                Data.MarkEventTypeProgress(progress);
                Data.Timestamp = DateTimeOffset.UtcNow;
                Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

                hasMoreRecords = indexRecordsResult.Records.Any();
            }

            Data.IsCompleted = true;
            Data.Timestamp = DateTimeOffset.UtcNow;

            Data = await cluster.PingAsync(Data).ConfigureAwait(false);
            logger.Info(() => $"The job has been completed.");

            return JobExecutionStatus.Completed;
        }
    }

    public class ReplayPublicEvents_JobFactory
    {
        private readonly ReplayPublicEvents_Job job;
        private readonly CronusContext context;
        private readonly BoundedContext boundedContext;

        public ReplayPublicEvents_JobFactory(ReplayPublicEvents_Job job, IOptions<BoundedContext> boundedContext, CronusContext context)
        {
            this.job = job;
            this.context = context;
            this.boundedContext = boundedContext.Value;
        }

        public ReplayPublicEvents_Job CreateJob(ReplayPublicEventsRequested signal)
        {
            job.Name = $"urn:{boundedContext.Name}:{context.Tenant}:{job.Name}:{signal.RecipientBoundedContext}:{signal.RecipientHandlers}:{signal.SourceEventTypeId}";

            job.BuildInitialData(() => new ReplayPublicEvents_JobData()
            {
                After = signal.ReplayOptions.After,
                Before = signal.ReplayOptions.Before,
                RecipientBoundedContext = signal.RecipientBoundedContext,
                RecipientHandlers = signal.RecipientHandlers,
                SourceEventTypeId = signal.SourceEventTypeId
            });

            return job;
        }
    }

    public class ReplayPublicEvents_JobData : IJobData
    {
        public ReplayPublicEvents_JobData()
        {
            IsCompleted = false;
            Timestamp = DateTimeOffset.UtcNow;
            DueDate = DateTimeOffset.MaxValue;
        }

        public bool IsCompleted { get; set; }

        public EventTypePagingProgress EventTypePaging { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public DateTimeOffset DueDate { get; set; }

        public DateTimeOffset? After { get; set; }
        public DateTimeOffset? Before { get; set; }
        public string RecipientBoundedContext { get; set; }
        public string RecipientHandlers { get; set; }
        public string SourceEventTypeId { get; set; }

        public class EventTypePagingProgress
        {
            public EventTypePagingProgress(string eventTypeId, string paginationToken, long processedCount, long totalCount)
            {
                Type = eventTypeId;
                PaginationToken = paginationToken;
                ProcessedCount = processedCount;
                TotalCount = totalCount;
            }

            public string Type { get; set; }

            public string PaginationToken { get; set; }

            public long ProcessedCount { get; set; }

            public long TotalCount { get; set; }
        }

        public void MarkEventTypeProgress(EventTypePagingProgress progress)
        {
            if (EventTypePaging is null)
            {
                EventTypePaging = progress;
            }
            else
            {
                EventTypePaging.PaginationToken = progress.PaginationToken;
                EventTypePaging.ProcessedCount += progress.ProcessedCount;
                EventTypePaging.TotalCount = progress.TotalCount;
            }
        }

        //public RebuildProjectionProgress GetProgressSignal(string tenant)
        //{
        //    return new RebuildProjectionProgress(tenant, Version.ProjectionName, EventTypePaging.Sum(x => x.ProcessedCount), EventTypePaging.Sum(x => x.TotalCount));
        //}

        //public RebuildProjectionFinished GetProgressFinishedSignal(string tenant)
        //{
        //    return new RebuildProjectionFinished(tenant, Version.ProjectionName);
        //}

        //public RebuildProjectionStarted GetProgressStartedSignal(string tenant)
        //{
        //    return new RebuildProjectionStarted(tenant, Version.ProjectionName);
        //}
    }
}
