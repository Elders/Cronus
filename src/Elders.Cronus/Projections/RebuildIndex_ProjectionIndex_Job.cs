using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.EventStore.Index.Handlers;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public class SystemProjectionHelper
    {
        private readonly ProjectionHasher projectionHasher;

        public SystemProjectionHelper(ProjectionHasher projectionHasher)
        {
            this.projectionHasher = projectionHasher;
        }

        public ProjectionVersion GetProjectionVersionsVersion()
        {
            return new ProjectionVersion(ProjectionVersionsHandler.ContractId, ProjectionStatus.Live, 1, projectionHasher.CalculateHash(typeof(ProjectionVersionsHandler)));
        }
    }

    public class RebuildIndex_ProjectionIndex_Job : CronusJob<RebuildProjectionIndex_JobData>
    {
        private readonly SystemProjectionHelper systemProjectionHelper;
        private readonly IPublisher<ISystemSignal> signalPublisher;
        private readonly IInitializableProjectionStore projectionStoreInitializer;
        private readonly IEventStore eventStore;
        private readonly ProjectionIndex index;
        private readonly EventToAggregateRootId eventToAggregateIndex;
        private readonly IProjectionReader projectionReader;
        private readonly CronusContext context;
        private readonly IMessageCounter messageCounter;

        public RebuildIndex_ProjectionIndex_Job(SystemProjectionHelper systemProjectionHelper, IPublisher<ISystemSignal> signalPublisher, IInitializableProjectionStore projectionStoreInitializer, EventStoreFactory eventStoreFactory, ProjectionIndex index, EventToAggregateRootId eventToAggregateIndex, IProjectionReader projectionReader, CronusContext context, IMessageCounter messageCounter, ILogger<RebuildIndex_ProjectionIndex_Job> logger) : base(logger)
        {
            this.systemProjectionHelper = systemProjectionHelper;
            this.signalPublisher = signalPublisher;
            this.projectionStoreInitializer = projectionStoreInitializer;
            this.eventStore = eventStoreFactory.GetEventStore();
            this.index = index;
            this.eventToAggregateIndex = eventToAggregateIndex;
            this.projectionReader = projectionReader;
            this.context = context;
            this.messageCounter = messageCounter;
        }

        public override string Name { get; set; } = typeof(ProjectionIndex).GetContractId();

        protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            ProjectionVersion version = Data.Version;

            if (IsVersionTrackerMissing())
            {
                if (version.ProjectionName.Equals(ProjectionVersionsHandler.ContractId, StringComparison.OrdinalIgnoreCase))
                {
                    ProjectionVersion persistentVersion = systemProjectionHelper.GetProjectionVersionsVersion();
                    projectionStoreInitializer.Initialize(persistentVersion);
                }
                else
                {
                    return JobExecutionStatus.Running;
                }
            }

            Type projectionType = version.ProjectionName.GetTypeByContract();
            IndexStatus indexStatus = GetIndexStatus<EventToAggregateRootId>();

            if (indexStatus.IsNotPresent() && IsNotSystemProjection(projectionType)) return JobExecutionStatus.Running;// ReplayResult.RetryLater($"The index is not present");
            if (IsVersionTrackerMissing() && IsNotSystemProjection(projectionType)) return JobExecutionStatus.Running;// ReplayResult.RetryLater($"Projection `{version}` still don't have present index."); //WHEN TO RETRY AGAIN
            if (HasReplayTimeout(Data.DueDate)) return JobExecutionStatus.Failed;// ReplayResult.Timeout($"Rebuild of projection `{version}` has expired. Version:{version} Deadline:{Data.DueDate}.");

            var allVersions = GetAllVersions(version);
            if (allVersions.IsOutdatad(version)) return JobExecutionStatus.Failed;// new ReplayResult($"Version `{version}` is outdated. There is a newer one which is already live.");
            if (allVersions.IsCanceled(version)) return JobExecutionStatus.Failed;// new ReplayResult($"Version `{version}` was canceled.");

            Dictionary<int, string> processedAggregates = new Dictionary<int, string>();

            projectionStoreInitializer.Initialize(version);

            var startSignal = Data.GetProgressStartedSignal(context.Tenant);
            signalPublisher.Publish(startSignal);

            IEnumerable<Type> projectionHandledEventTypes = GetInvolvedEventTypes(projectionType);
            foreach (Type eventType in projectionHandledEventTypes)
            {
                string eventTypeId = eventType.GetContractId();
                bool hasMoreRecords = true;
                while (hasMoreRecords && Data.IsCompleted == false)
                {
                    RebuildProjectionIndex_JobData.EventTypePagingProgress paging = Data.EventTypePaging.Where(et => et.Type.Equals(eventTypeId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    string paginationToken = paging?.PaginationToken;
                    LoadIndexRecordsResult indexRecordsResult = eventToAggregateIndex.EnumerateRecords(eventTypeId, paginationToken);

                    IEnumerable<IndexRecord> indexRecords = indexRecordsResult.Records;
                    long currentSessionProcessedCount = 0;
                    foreach (IndexRecord indexRecord in indexRecords)
                    {
                        try
                        {
                            currentSessionProcessedCount++;

                            #region TrackAggregate
                            int aggreagteRootIdHash = indexRecord.AggregateRootId.GetHashCode();
                            if (processedAggregates.ContainsKey(aggreagteRootIdHash))
                                continue;
                            processedAggregates.Add(aggreagteRootIdHash, null);
                            #endregion

                            string mess = Encoding.UTF8.GetString(indexRecord.AggregateRootId);
                            IAggregateRootId arId = GetAggregateRootId(mess);
                            EventStream stream = eventStore.Load(arId);

                            foreach (AggregateCommit arCommit in stream.Commits)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    logger.Info(() => $"Job has been cancelled.");
                                    return JobExecutionStatus.Running;
                                }

                                index.Index(arCommit, version);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.WarnException(ex, () => $"{indexRecord.AggregateRootId} was skipped when rebuilding {version.ProjectionName}.");
                        }
                    }
                    long totalEvents = messageCounter.GetCount(eventType);
                    var progress = new RebuildProjectionIndex_JobData.EventTypePagingProgress(eventTypeId, indexRecordsResult.PaginationToken, currentSessionProcessedCount, totalEvents);

                    Data.MarkEventTypeProgress(progress);
                    Data.Timestamp = DateTimeOffset.UtcNow;
                    Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

                    hasMoreRecords = indexRecordsResult.Records.Any();

                    var progressSignal = Data.GetProgressSignal(context.Tenant);
                    signalPublisher.Publish(progressSignal);
                }
            }

            Data.IsCompleted = true;
            Data.Timestamp = DateTimeOffset.UtcNow;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            var finishSignal = Data.GetProgressFinishedSignal(context.Tenant);
            signalPublisher.Publish(finishSignal);

            logger.Info(() => $"The job has been completed.");

            return JobExecutionStatus.Completed;
        }

        IEnumerable<Type> GetInvolvedEventTypes(Type projectionType)
        {
            var ieventHandler = typeof(IEventHandler<>);
            var interfaces = projectionType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().First();
                yield return eventType;
            }
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

        bool IsNotSystemProjection(Type projectionType)
        {
            return typeof(ISystemProjection).IsAssignableFrom(projectionType) == false;
        }

        bool IsVersionTrackerMissing()
        {
            var versionId = new ProjectionVersionManagerId(ProjectionVersionsHandler.ContractId, context.Tenant);
            var result = projectionReader.Get<ProjectionVersionsHandler>(versionId);

            return result.HasError || result.NotFound;
        }

        private bool HasReplayTimeout(DateTimeOffset replayUntil)
        {
            return DateTimeOffset.UtcNow >= replayUntil;
        }

        ProjectionVersions GetAllVersions(ProjectionVersion version)
        {
            var versionId = new ProjectionVersionManagerId(version.ProjectionName, context.Tenant);
            var result = projectionReader.Get<ProjectionVersionsHandler>(versionId);
            if (result.IsSuccess)
                return result.Data.State.AllVersions;

            return new ProjectionVersions();
        }

        protected override RebuildProjectionIndex_JobData Override(RebuildProjectionIndex_JobData fromCluster, RebuildProjectionIndex_JobData fromLocal)
        {
            if ((fromCluster.IsCompleted && fromCluster.Timestamp < fromLocal.Timestamp) || fromCluster.Version < fromLocal.Version)
                return fromLocal;
            else
                return fromCluster;
        }
    }

    public class RebuildIndex_ProjectionIndex_JobFactory
    {
        private readonly RebuildIndex_ProjectionIndex_Job job;
        private readonly CronusContext context;
        private readonly BoundedContext boundedContext;

        public RebuildIndex_ProjectionIndex_JobFactory(RebuildIndex_ProjectionIndex_Job job, IOptions<BoundedContext> boundedContext, CronusContext context)
        {
            this.job = job;
            this.context = context;
            this.boundedContext = boundedContext.Value;
        }

        public RebuildIndex_ProjectionIndex_Job CreateJob(ProjectionVersion version, VersionRequestTimebox timebox)
        {
            job.Name = $"urn:{boundedContext.Name}:{context.Tenant}:{job.Name}:{version.ProjectionName}_{version.Hash}_{version.Revision}";

            job.BuildInitialData(() =>
            {
                var data = new RebuildProjectionIndex_JobData();
                data.Timestamp = timebox.RequestStartAt;
                data.DueDate = timebox.FinishRequestUntil;
                data.Version = version;

                Type projectionType = version.ProjectionName.GetTypeByContract();
                IEnumerable<Type> projectionHandledEventTypes = GetInvolvedEventTypes(projectionType);
                //foreach (Type eventType in projectionHandledEventTypes)
                //{
                //    long totalEvents = messageCounter.GetCount(eventType);
                //    var progress = new RebuildProjectionIndex_JobData.EventTypePagingProgress(eventType.GetContractId(), string.Empty, 0, totalEvents);
                //    dataOverride.Init(progress);
                //}

                return data;
            });

            return job;
        }

        IEnumerable<Type> GetInvolvedEventTypes(Type projectionType)
        {
            var ieventHandler = typeof(IEventHandler<>);
            var interfaces = projectionType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().First();
                yield return eventType;
            }
        }
    }

    public class RebuildProjectionIndex_JobData : IJobData
    {
        public RebuildProjectionIndex_JobData()
        {
            IsCompleted = false;
            EventTypePaging = new List<EventTypePagingProgress>();
            Timestamp = DateTimeOffset.UtcNow;
            DueDate = DateTimeOffset.MaxValue;
        }

        public bool IsCompleted { get; set; }

        public List<EventTypePagingProgress> EventTypePaging { get; set; }

        public ProjectionVersion Version { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public DateTimeOffset DueDate { get; set; }

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
            EventTypePagingProgress existing = EventTypePaging.Where(et => et.Type.Equals(progress.Type, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (existing is null)
            {
                EventTypePaging.Add(progress);
            }
            else
            {
                existing.PaginationToken = progress.PaginationToken;
                existing.ProcessedCount += progress.ProcessedCount;
                existing.TotalCount = progress.TotalCount;
            }
        }

        public void Init(EventTypePagingProgress progress)
        {
            EventTypePagingProgress existing = EventTypePaging.Where(et => et.Type.Equals(progress.Type, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (existing is null)
            {
                EventTypePaging.Add(progress);
            }
        }

        public RebuildProjectionProgress GetProgressSignal(string tenant)
        {
            return new RebuildProjectionProgress(tenant, Version.ProjectionName, EventTypePaging.Sum(x => x.ProcessedCount), EventTypePaging.Sum(x => x.TotalCount));
        }

        public RebuildProjectionFinished GetProgressFinishedSignal(string tenant)
        {
            return new RebuildProjectionFinished(tenant, Version.ProjectionName);
        }

        public RebuildProjectionStarted GetProgressStartedSignal(string tenant)
        {
            return new RebuildProjectionStarted(tenant, Version.ProjectionName);
        }
    }

    [DataContract(Name = "373f4ff0-cb6f-499e-9fa5-1666ccc00689")]
    public class RebuildProjectionProgress : ISystemSignal
    {
        public RebuildProjectionProgress() { }

        public RebuildProjectionProgress(string tenant, string projectionTypeId, long processedCount, long totalCount)
        {
            Tenant = tenant;
            ProjectionTypeId = projectionTypeId;
            ProcessedCount = processedCount;
            TotalCount = totalCount;
        }

        [DataMember(Order = 0)]
        public string Tenant { get; set; }

        [DataMember(Order = 1)]
        public string ProjectionTypeId { get; set; }

        [DataMember(Order = 2)]
        public long ProcessedCount { get; set; }

        [DataMember(Order = 3)]
        public long TotalCount { get; set; }
    }

    [DataContract(Name = "b03199e7-2752-48b7-93de-c45ad18b55bf")]
    public class RebuildProjectionStarted : ISystemSignal
    {
        public RebuildProjectionStarted() { }

        public RebuildProjectionStarted(string tenant, string projectionTypeId)
        {
            Tenant = tenant;
            ProjectionTypeId = projectionTypeId;
        }

        [DataMember(Order = 0)]
        public string Tenant { get; set; }

        [DataMember(Order = 1)]
        public string ProjectionTypeId { get; set; }
    }

    [DataContract(Name = "b248432b-451c-4894-84f2-c5ac5bc35139")]
    public class RebuildProjectionFinished : ISystemSignal
    {
        public RebuildProjectionFinished() { }

        public RebuildProjectionFinished(string tenant, string projectionTypeId)
        {
            Tenant = tenant;
            ProjectionTypeId = projectionTypeId;
        }

        [DataMember(Order = 0)]
        public string Tenant { get; set; }

        [DataMember(Order = 1)]
        public string ProjectionTypeId { get; set; }
    }
}
