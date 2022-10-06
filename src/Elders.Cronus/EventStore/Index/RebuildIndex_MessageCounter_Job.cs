using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore.Index.Handlers;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    public class RebuildIndex_MessageCounter_Job : CronusJob<RebuildEventCounterIndex_JobData>
    {
        private readonly CronusContext context;
        private readonly TypeContainer<IEvent> eventTypes;
        private readonly IMessageCounter messageCounter;
        private readonly IProjectionReader projectionReader;
        private readonly IIndexStore indexStore;

        public RebuildIndex_MessageCounter_Job(CronusContext context, TypeContainer<IEvent> eventTypes, IMessageCounter eventCounter, IProjectionReader projectionReader, IIndexStore indexStore, ILogger<RebuildIndex_MessageCounter_Job> logger) : base(logger)
        {
            this.context = context;
            this.eventTypes = eventTypes;
            this.messageCounter = eventCounter;
            this.projectionReader = projectionReader;
            this.indexStore = indexStore;
        }

        public override string Name { get; set; } = typeof(MessageCounterIndex).GetContractId();

        protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            // mynkow. this one fails
            IndexStatus indexStatus = await GetIndexStatusAsync<EventToAggregateRootId>().ConfigureAwait(false);
            if (indexStatus.IsNotPresent()) return JobExecutionStatus.Running;

            //projectionStoreInitializer.Initialize(version);

            
            foreach (Type eventType in eventTypes.Items)
            {
                var pingSource = new CancellationTokenSource();

                string eventTypeId = eventType.GetContractId();

                logger.Info(() => $"Message counter for {eventTypeId} has been reset");

               
                CancellationToken ct = pingSource.Token;

                Task.Factory.StartNew(async () =>
                {
                    logger.Info(() => $"Message counter cluster ping for {eventTypeId}.");
                    await cluster.PingAsync<RebuildEventCounterIndex_JobData>(ct).ConfigureAwait(false);
                    await Task.Delay(15000, ct).ConfigureAwait(false);
                }, ct).ConfigureAwait(false);

                // Maybe we should move this to a BeforeRun method.
                await messageCounter.ResetAsync(eventType).ConfigureAwait(false);

                long count = await indexStore.GetCountAsync(eventTypeId).ConfigureAwait(false);
                await messageCounter.IncrementAsync(eventType, count).ConfigureAwait(false);

                pingSource.Cancel();
            }

            Data.IsCompleted = true;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            logger.Info(() => $"The job has been completed.");

            return JobExecutionStatus.Completed;
        }


        async Task<IndexStatus> GetIndexStatusAsync<TIndex>() where TIndex : IEventStoreIndex
        {
            var id = new EventStoreIndexManagerId(typeof(TIndex).GetContractId(), context.Tenant);
            var result = await projectionReader.GetAsync<EventStoreIndexStatus>(id).ConfigureAwait(false);
            if (result.IsSuccess)
                return result.Data.State.Status;

            return IndexStatus.NotPresent;
        }
    }

    public class RebuildIndex_MessageCounter_JobFactory
    {
        private readonly RebuildIndex_MessageCounter_Job job;
        private readonly CronusContext context;
        private readonly BoundedContext boundedContext;

        public RebuildIndex_MessageCounter_JobFactory(RebuildIndex_MessageCounter_Job job, IOptions<BoundedContext> boundedContext, CronusContext context)
        {
            this.job = job;
            this.context = context;
            this.boundedContext = boundedContext.Value;
        }

        public RebuildIndex_MessageCounter_Job CreateJob(VersionRequestTimebox timebox)
        {
            job.Name = $"urn:{boundedContext.Name}:{context.Tenant}:{job.Name}";
            job.BuildInitialData(() => new RebuildEventCounterIndex_JobData()
            {
                Timestamp = timebox.RequestStartAt
            });

            return job;
        }
    }

    public class RebuildEventCounterIndex_JobData : IJobData
    {
        public RebuildEventCounterIndex_JobData()
        {
            IsCompleted = false;
            EventTypePaging = new List<EventTypeRebuildPaging>();
            Timestamp = DateTimeOffset.UtcNow;
            DueDate = DateTimeOffset.MaxValue;
        }

        public bool IsCompleted { get; set; }

        public List<EventTypeRebuildPaging> EventTypePaging { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public DateTimeOffset DueDate { get; set; }

        public class EventTypeRebuildPaging
        {
            public string Type { get; set; }

            public string PaginationToken { get; set; }
        }

        public void MarkPaginationTokenAsProcessed(string eventTypeId, string paginationToken)
        {
            EventTypeRebuildPaging existing = EventTypePaging.Where(et => et.Type.Equals(eventTypeId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (existing is null)
            {
                existing = new EventTypeRebuildPaging()
                {
                    Type = eventTypeId,
                    PaginationToken = paginationToken
                };

                EventTypePaging.Add(existing);
            }
            else
            {
                existing.PaginationToken = paginationToken;
            }
        }
    }
}
