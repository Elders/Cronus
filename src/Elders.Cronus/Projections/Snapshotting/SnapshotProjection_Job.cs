//using Elders.Cronus.Cluster.Job;
//using Elders.Cronus.EventStore;
//using Elders.Cronus.EventStore.Index;
//using Elders.Cronus.MessageProcessing;
//using Elders.Cronus.Projections.Versioning;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Elders.Cronus.Projections
//{
//    public class SnapshotProjection_Job : CronusJob<SnapshotProjection_JobData>
//    {
//        private readonly IPublisher<ISignal> signalPublisher;
//        private readonly IInitializableProjectionStore projectionStoreInitializer;
//        private readonly IEventStore eventStore;
//        private readonly ProjectionIndex index;
//        private readonly EventToAggregateRootId eventToAggregateIndex;
//        private readonly IProjectionReader projectionReader;
//        private readonly CronusContext context;
//        private readonly IMessageCounter messageCounter;
//        private readonly ILogger<SnapshotProjection_Job> logger;

//        public SnapshotProjection_Job(IPublisher<ISignal> signalPublisher, IInitializableProjectionStore projectionStoreInitializer, IEventStore eventStore, ProjectionIndex index, EventToAggregateRootId eventToAggregateIndex, IProjectionReader projectionReader, CronusContext context, IMessageCounter messageCounter, ILogger<SnapshotProjection_Job> logger) : base(logger)
//        {
//            this.signalPublisher = signalPublisher;
//            this.projectionStoreInitializer = projectionStoreInitializer;
//            this.eventStore = eventStore;
//            this.index = index;
//            this.eventToAggregateIndex = eventToAggregateIndex;
//            this.projectionReader = projectionReader;
//            this.context = context;
//            this.messageCounter = messageCounter;
//            this.logger = logger;
//        }

//        public override string Name { get; set; } = typeof(ProjectionIndex).GetContractId();

//        protected override Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
//        {
//            return Task.FromResult(JobExecutionStatus.Completed);
//        }
//    }

//    public class Snapshot_ProjectionSnapshot_JobFactory
//    {
//        private readonly SnapshotProjection_Job job;
//        private readonly CronusContext context;
//        private readonly BoundedContext boundedContext;

//        public Snapshot_ProjectionSnapshot_JobFactory(SnapshotProjection_Job job, IOptions<BoundedContext> boundedContext, CronusContext context)
//        {
//            this.job = job;
//            this.context = context;
//            this.boundedContext = boundedContext.Value;
//        }

//        public SnapshotProjection_Job CreateJob(ProjectionVersion version, VersionRequestTimebox timebox)
//        {
//            job.Name = $"urn:{boundedContext.Name}:{context.Tenant}:{job.Name}:{version.ProjectionName}_{version.Hash}_{version.Revision}";

//            return job;
//        }
//    }

//    public class SnapshotProjection_JobData : IJobData
//    {
//        public SnapshotProjection_JobData()
//        {
//            IsCompleted = false;
//            EventTypePaging = new List<EventTypePagingProgress>();
//            Timestamp = DateTimeOffset.UtcNow;
//            DueDate = DateTimeOffset.MaxValue;
//        }

//        public bool IsCompleted { get; set; }

//        public List<EventTypePagingProgress> EventTypePaging { get; set; }

//        public ProjectionVersion Version { get; set; }

//        public DateTimeOffset Timestamp { get; set; }

//        public DateTimeOffset DueDate { get; set; }

//        public class EventTypePagingProgress
//        {
//            public EventTypePagingProgress(string eventTypeId, string paginationToken, long processedCount, long totalCount)
//            {
//                Type = eventTypeId;
//                PaginationToken = paginationToken;
//                ProcessedCount = processedCount;
//                TotalCount = totalCount;
//            }

//            public string Type { get; set; }

//            public string PaginationToken { get; set; }

//            public long ProcessedCount { get; set; }

//            public long TotalCount { get; set; }
//        }

//        public void MarkEventTypeProgress(EventTypePagingProgress progress)
//        {
//            EventTypePagingProgress existing = EventTypePaging.Where(et => et.Type.Equals(progress.Type, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
//            if (existing is null)
//            {
//                EventTypePaging.Add(progress);
//            }
//            else
//            {
//                existing.PaginationToken = progress.PaginationToken;
//                existing.ProcessedCount += progress.ProcessedCount;
//                existing.TotalCount = progress.TotalCount;
//            }
//        }

//        public void Init(EventTypePagingProgress progress)
//        {
//            EventTypePagingProgress existing = EventTypePaging.Where(et => et.Type.Equals(progress.Type, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
//            if (existing is null)
//            {
//                EventTypePaging.Add(progress);
//            }
//        }
//    }
//}
