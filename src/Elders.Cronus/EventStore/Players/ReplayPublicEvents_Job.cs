using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.EventStore.Players
{
    public class ReplayPublicEvents_Job : CronusJob<ReplayPublicEvents_JobData>
    {
        private readonly IPublisher<IPublicEvent> publicEventPublisher;
        private readonly ICronusContextAccessor contextAccessor;
        private readonly IEventStorePlayer player;

        public ReplayPublicEvents_Job(IPublisher<IPublicEvent> publicEventPublisher, ICronusContextAccessor contextAccessor, IEventStorePlayer eventStorePlayer, ILogger<ReplayPublicEvents_Job> logger) : base(logger)
        {
            this.publicEventPublisher = publicEventPublisher;
            this.contextAccessor = contextAccessor;
            this.player = eventStorePlayer;
        }
        public override string Name { get; set; } = "c0e0f5fc-1f22-4022-96d0-bf02590951d6";

        protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            if (Data.IsCompleted)
                return JobExecutionStatus.Completed;

            PlayerOptions opt = new PlayerOptions()
            {
                EventTypeId = Data.SourceEventTypeId,
                PaginationToken = Data.EventTypePaging?.PaginationToken,
                After = Data.After,
                Before = Data.Before ?? DateTimeOffset.UtcNow
            };

            Type messageType = Data.SourceEventTypeId.GetTypeByContract();
            string boundedContext = messageType.GetBoundedContext();

            ulong counter = Data.EventTypePaging is null ? 0 : Data.EventTypePaging.ProcessedCount;
            PlayerOperator @operator = new PlayerOperator()
            {
                OnLoadAsync = eventRaw =>
                {
                    string tenant = contextAccessor.CronusContext.Tenant;
                    string messageId = $"urn:cronus:{boundedContext}:{tenant}:{Guid.NewGuid()}";
                    //TODO: Document which headers are essential or make another ctor for CronusMessage with byte[]
                    var headers = new Dictionary<string, string>()
                    {
                        { MessageHeader.MessageId, messageId },
                        { MessageHeader.RecipientBoundedContext, Data.RecipientBoundedContext },
                        { MessageHeader.RecipientHandlers, Data.RecipientHandlers },
                        { MessageHeader.PublishTimestamp, DateTime.UtcNow.ToFileTimeUtc().ToString() },
                        { MessageHeader.Tenant, tenant },
                        { MessageHeader.BoundedContext, boundedContext },
                        { "contract_name",  Data.SourceEventTypeId }
                    };

                    publicEventPublisher.Publish(eventRaw.Data, Data.SourceEventTypeId.GetTypeByContract(), tenant, headers);

                    counter++;
                    return Task.CompletedTask;
                },
                NotifyProgressAsync = async options =>
                {
                    var progress = new ReplayPublicEvents_JobData.EventPaging(options.EventTypeId, options.PaginationToken, options.After, options.Before, counter, 0);
                    Data.EventTypePaging = progress;
                    Data.Timestamp = DateTimeOffset.UtcNow;
                    Data = await cluster.PingAsync(Data).ConfigureAwait(false);
                }
            };

            await player.EnumerateEventStore(@operator, opt).ConfigureAwait(false);

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
        private readonly ICronusContextAccessor contextAccessor;
        private readonly BoundedContext boundedContext;

        public ReplayPublicEvents_JobFactory(ReplayPublicEvents_Job job, IOptions<BoundedContext> boundedContext, ICronusContextAccessor contextAccessor)
        {
            this.job = job;
            this.contextAccessor = contextAccessor;
            this.boundedContext = boundedContext.Value;
        }

        public ReplayPublicEvents_Job CreateJob(ReplayPublicEventsRequested signal)
        {
            job.Name = $"urn:{boundedContext.Name}:{contextAccessor.CronusContext.Tenant}:{job.Name}:{signal.RecipientBoundedContext}:{signal.RecipientHandlers}:{signal.SourceEventTypeId}";

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

        public EventPaging EventTypePaging { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public DateTimeOffset DueDate { get; set; }

        public DateTimeOffset? After { get; set; }
        public DateTimeOffset? Before { get; set; }
        public string RecipientBoundedContext { get; set; }
        public string RecipientHandlers { get; set; }
        public string SourceEventTypeId { get; set; }

        public class EventPaging
        {
            public EventPaging(string eventTypeId, string paginationToken, DateTimeOffset? after, DateTimeOffset? before, ulong processedCount, ulong totalCount)
            {
                Type = eventTypeId;
                PaginationToken = paginationToken;
                After = after;
                Before = before;
                ProcessedCount = processedCount;
                TotalCount = totalCount;
            }

            public string Type { get; set; }

            public string PaginationToken { get; set; }

            public DateTimeOffset? After { get; set; }
            public DateTimeOffset? Before { get; set; }

            public ulong ProcessedCount { get; set; }

            public ulong TotalCount { get; set; }
        }
    }
}
