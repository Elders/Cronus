using Elders.Cronus.Cluster.Job;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Players
{
    public class ReplayPublicEvents_Job : CronusJob<ReplayPublicEvents_JobData>
    {
        private readonly IPublisher<IPublicEvent> publicEventPublisher;
        private readonly ISerializer serializer;
        private readonly IEventStorePlayer player;

        public ReplayPublicEvents_Job(IPublisher<IPublicEvent> publicEventPublisher, ISerializer serializer, IEventStorePlayer eventStorePlayer, ILogger<ReplayPublicEvents_Job> logger) : base(logger)
        {
            this.publicEventPublisher = publicEventPublisher;
            this.serializer = serializer;
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

            var headers = new Dictionary<string, string>()
            {
                {MessageHeader.RecipientBoundedContext, Data.RecipientBoundedContext},
                {MessageHeader.RecipientHandlers, Data.RecipientHandlers}
            };

            uint counter = 0;
            PlayerOperator @operator = new PlayerOperator()
            {
                OnLoadAsync = eventRaw =>
                {
                    using (var stream = new MemoryStream(eventRaw.Data))
                    {
                        if (serializer.Deserialize(stream) is IPublicEvent publicEvent)
                        {
                            publicEventPublisher.Publish(publicEvent, headers);
                        }
                    }
                    counter++;
                    return Task.CompletedTask;
                },
                NotifyProgressAsync = async options =>
                {
                    var progress = new ReplayPublicEvents_JobData.EventPaging(options.EventTypeId, options.PaginationToken, options.After, options.Before, 0, 0);
                    Data.Timestamp = DateTimeOffset.UtcNow;
                    Data = await cluster.PingAsync(Data);
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
