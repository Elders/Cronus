using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.EventStore.Index.Handlers;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    public class RebuildIndex_ProjectionIndex_Job : CronusJob<RebuildProjectionIndex_JobData>
    {
        private readonly IInitializableProjectionStore projectionStoreInitializer;
        private readonly IEventStore eventStore;
        private readonly IEventStorePlayer eventStorePlayer;
        private readonly ProjectionIndex index;
        private readonly IProjectionWriter projectionWriter;
        private readonly EventToAggregateRootId eventToAggregateIndex;
        private readonly IProjectionReader projectionReader;
        private readonly CronusContext context;

        public RebuildIndex_ProjectionIndex_Job(IInitializableProjectionStore projectionStoreInitializer, IEventStore eventStore, IEventStorePlayer eventStorePlayer, ProjectionIndex index, IProjectionWriter projectionWriter, EventToAggregateRootId eventToAggregateIndex, IProjectionReader projectionReader, CronusContext context)
        {
            this.projectionStoreInitializer = projectionStoreInitializer;
            this.eventStore = eventStore;
            this.eventStorePlayer = eventStorePlayer;
            this.index = index;
            this.projectionWriter = projectionWriter;
            this.eventToAggregateIndex = eventToAggregateIndex;
            this.projectionReader = projectionReader;
            this.context = context;
        }

        public override string Name { get; set; } = typeof(ProjectionIndex).GetContractId();

        protected override RebuildProjectionIndex_JobData BuildInitialData() => new RebuildProjectionIndex_JobData();

        protected override async Task<JobExecutionStatus> RunJob(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            ProjectionVersion version = Data.Version;
            Type projectionType = version.ProjectionName.GetTypeByContract();

            // mynkow. this one fails
            IndexStatus indexStatus = GetIndexStatus<EventToAggregateRootId>();
            if (indexStatus.IsNotPresent() && IsNotSystemProjection(projectionType)) return JobExecutionStatus.Running;// ReplayResult.RetryLater($"The index is not present");

            if (IsVersionTrackerMissing() && IsNotSystemProjection(projectionType)) return JobExecutionStatus.Running;// ReplayResult.RetryLater($"Projection `{version}` still don't have present index."); //WHEN TO RETRY AGAIN
            if (HasReplayTimeout(Data.DueDate)) return JobExecutionStatus.Failed;// ReplayResult.Timeout($"Rebuild of projection `{version}` has expired. Version:{version} Deadline:{Data.DueDate}.");

            var allVersions = GetAllVersions(version);
            if (allVersions.IsOutdatad(version)) return JobExecutionStatus.Failed;// new ReplayResult($"Version `{version}` is outdated. There is a newer one which is already live.");
            if (allVersions.IsCanceled(version)) return JobExecutionStatus.Failed;// new ReplayResult($"Version `{version}` was canceled.");

            Dictionary<int, string> processedAggregates = new Dictionary<int, string>();

            projectionStoreInitializer.Initialize(version);

            IEnumerable<string> projectionHandledEventTypes = GetInvolvedEvents(projectionType);
            foreach (var eventType in projectionHandledEventTypes)
            {
                bool hasMoreRecords = true;
                while (hasMoreRecords && Data.IsCompleted == false)
                {
                    RebuildProjectionIndex_JobData.EventTypeRebuildPaging paging = Data.EventTypePaging.Where(et => et.Type.Equals(eventType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    string paginationToken = paging?.PaginationToken;
                    LoadIndexRecordsResult indexRecordsResult = eventToAggregateIndex.EnumerateRecords(eventType, paginationToken);

                    IEnumerable<IndexRecord> indexRecords = indexRecordsResult.Records;
                    foreach (IndexRecord indexRecord in indexRecords)
                    {
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
                            for (int i = 0; i < arCommit.Events.Count; i++)
                            {
                                IEvent theEvent = arCommit.Events[i].Unwrap();

                                if (projectionHandledEventTypes.Contains(theEvent.GetType().GetContractId())) // filters out the events which are not part of the projection
                                {
                                    var origin = new EventOrigin(mess, arCommit.Revision, i, arCommit.Timestamp);
                                    projectionWriter.Save(projectionType, theEvent, origin, version);
                                }
                            }
                        }
                    }

                    Data.MarkPaginationTokenAsProcessed(eventType, indexRecordsResult.PaginationToken);
                    Data = await cluster.PingAsync(Data).ConfigureAwait(false);

                    hasMoreRecords = indexRecordsResult.Records.Any();
                }
            }

            Data.IsCompleted = true;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            return JobExecutionStatus.Completed;
        }

        IEnumerable<string> GetInvolvedEvents(Type projectionType)
        {
            var ieventHandler = typeof(IEventHandler<>);
            var interfaces = projectionType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler);
            foreach (var @interface in interfaces)
            {
                Type eventType = @interface.GetGenericArguments().First();
                yield return eventType.GetContractId();
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

        public void SetProjection(ProjectionVersion version, VersionRequestTimebox timebox)
        {
            var dataOverride = BuildInitialData();
            dataOverride.Timestamp = timebox.RebuildStartAt;
            dataOverride.DueDate = timebox.RebuildFinishUntil;
            dataOverride.Version = version;

            OverrideData(fromCluster => Override(fromCluster, dataOverride));
        }

        private RebuildProjectionIndex_JobData Override(RebuildProjectionIndex_JobData fromCluster, RebuildProjectionIndex_JobData dataOverride)
        {
            if ((fromCluster.IsCompleted && fromCluster.Timestamp < dataOverride.Timestamp) || fromCluster.Version < dataOverride.Version)
                return dataOverride;
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
            job.SetProjection(version, timebox);

            return job;
        }
    }

    public class RebuildProjectionIndex_JobData
    {
        public RebuildProjectionIndex_JobData() : this(null) { }

        public RebuildProjectionIndex_JobData(ProjectionVersion version)
        {
            IsCompleted = false;
            EventTypePaging = new List<EventTypeRebuildPaging>();
            Version = version;
            Timestamp = DateTimeOffset.UtcNow;
            DueDate = DateTimeOffset.MaxValue;
        }

        public bool IsCompleted { get; set; }

        public List<EventTypeRebuildPaging> EventTypePaging { get; set; }

        public ProjectionVersion Version { get; set; }

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
