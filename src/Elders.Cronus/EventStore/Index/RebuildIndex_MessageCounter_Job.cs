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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    public class RebuildIndex_MessageCounter_Job : CronusJob<RebuildEventCounterIndex_JobData>
    {
        private readonly CronusContext context;
        private readonly TypeContainer<IEvent> eventTypes;
        private readonly IEventStore eventStore;
        private readonly IMessageCounter messageCounter;
        private readonly EventToAggregateRootId eventToAggregateIndex;
        private readonly IProjectionReader projectionReader;

        public RebuildIndex_MessageCounter_Job(CronusContext context, TypeContainer<IEvent> eventTypes, IEventStore eventStore, IMessageCounter eventCounter, EventToAggregateRootId eventToAggregateIndex, IProjectionReader projectionReader, ILogger<RebuildIndex_MessageCounter_Job> logger) : base(logger)
        {
            this.context = context;
            this.eventTypes = eventTypes;
            this.eventStore = eventStore;
            this.messageCounter = eventCounter;
            this.eventToAggregateIndex = eventToAggregateIndex;
            this.projectionReader = projectionReader;
        }

        public override string Name { get; set; } = typeof(MessageCounterIndex).GetContractId();

        protected override RebuildEventCounterIndex_JobData BuildInitialData() => new RebuildEventCounterIndex_JobData();

        protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            // mynkow. this one fails
            IndexStatus indexStatus = GetIndexStatus<EventToAggregateRootId>();
            if (indexStatus.IsNotPresent()) return JobExecutionStatus.Running;

            //projectionStoreInitializer.Initialize(version);

            foreach (Type eventType in eventTypes.Items)
            {
                string eventTypeId = eventType.GetContractId();

                bool hasMoreRecords = true;

                while (hasMoreRecords && Data.IsCompleted == false)
                {
                    RebuildEventCounterIndex_JobData.EventTypeRebuildPaging paging = Data.EventTypePaging.Where(et => et.Type.Equals(eventTypeId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    string paginationToken = paging?.PaginationToken;
                    if (string.IsNullOrEmpty(paginationToken))
                    {
                        logger.Info(() => $"Message counter for {eventTypeId} has been reset");
                        // Maybe we should move this to a BeforeRun method.
                        messageCounter.Reset(eventType);
                    }
                    LoadIndexRecordsResult indexRecordsResult = eventToAggregateIndex.EnumerateRecords(eventTypeId, paginationToken);

                    IEnumerable<IndexRecord> indexRecords = indexRecordsResult.Records;
                    long currentSessionProcessedCount = 0;
                    foreach (IndexRecord indexRecord in indexRecords)
                    {
                        currentSessionProcessedCount++;

                        string mess = Encoding.UTF8.GetString(indexRecord.AggregateRootId);
                        IAggregateRootId arId = GetAggregateRootId(mess);
                        EventStream stream = eventStore.Load(arId);

                        foreach (AggregateCommit arCommit in stream.Commits)
                        {
                            foreach (var @event in arCommit.Events)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    logger.Info(() => $"Job has been cancelled.");
                                    return JobExecutionStatus.Running;
                                }

                                if (eventTypeId.Equals(@event.GetType().GetContractId(), StringComparison.OrdinalIgnoreCase))
                                    messageCounter.Increment(eventType);
                            }
                        }
                    }

                    Data.MarkPaginationTokenAsProcessed(eventTypeId, indexRecordsResult.PaginationToken);
                    Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

                    hasMoreRecords = indexRecordsResult.Records.Any();
                }
            }

            Data.IsCompleted = true;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            logger.Info(() => $"The job has been completed.");

            return JobExecutionStatus.Completed;
        }

        IAggregateRootId GetAggregateRootId(string mess)
        {
            var parts = mess.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                AggregateUrn urn;
                if (AggregateUrn.TryParse(part, out urn))
                {
                    return new AggregateRootId(urn.AggregateRootName, urn);
                }
                else
                {
                    byte[] raw = Convert.FromBase64String(part);
                    string urnString = Encoding.UTF8.GetString(raw);
                    if (AggregateUrn.TryParse(urnString, out urn))
                    {
                        return new AggregateRootId(urn.AggregateRootName, urn);
                    }
                }
            }

            throw new ArgumentException($"Invalid aggregate root id: {mess}", nameof(mess));
        }

        IndexStatus GetIndexStatus<TIndex>() where TIndex : IEventStoreIndex
        {
            var id = new EventStoreIndexManagerId(typeof(TIndex).GetContractId(), context.Tenant);
            var result = projectionReader.Get<EventStoreIndexStatus>(id);
            if (result.IsSuccess)
                return result.Data.State.Status;

            return IndexStatus.NotPresent;
        }

        public void SetTimeBox(VersionRequestTimebox timebox)
        {
            var dataOverride = BuildInitialData();
            dataOverride.Timestamp = timebox.RebuildStartAt;

            OverrideData(fromCluster => Override(fromCluster, dataOverride));
        }

        private RebuildEventCounterIndex_JobData Override(RebuildEventCounterIndex_JobData fromCluster, RebuildEventCounterIndex_JobData dataOverride)
        {
            if (fromCluster.IsCompleted && fromCluster.Timestamp < dataOverride.Timestamp)
                return dataOverride;
            else
                return fromCluster;
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
            job.SetTimeBox(timebox);

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
