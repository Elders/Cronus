//using Elders.Cronus.Cluster.Job;
//using Elders.Cronus.EventStore;
//using Elders.Cronus.EventStore.Index;
//using Elders.Cronus.EventStore.Index.Handlers;
//using Elders.Cronus.MessageProcessing;
//using Elders.Cronus.Projections.Versioning;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Elders.Cronus.Projections
//{
//    public class RebuildIndex_ProjectionIndex_Job : CronusJob<RebuildProjectionIndex_JobData>
//    {
//        private readonly IEventStorePlayer eventStorePlayer;
//        private readonly EventToAggregateRootId index;
//        private readonly IProjectionReader projectionReader;
//        private readonly CronusContext context;

//        public RebuildIndex_ProjectionIndex_Job(IEventStorePlayer eventStorePlayer, EventToAggregateRootId index, IProjectionReader projectionReader, CronusContext context)
//        {
//            this.eventStorePlayer = eventStorePlayer;
//            this.index = index;
//            this.projectionReader = projectionReader;
//            this.context = context;
//        }

//        public override string Name => typeof(ProjectionIndex).GetContractId();

//        protected override RebuildProjectionIndex_JobData BuildInitialData() => new RebuildProjectionIndex_JobData();

//        protected override async Task<JobExecutionStatus> RunJob(IClusterOperations cluster, CancellationToken cancellationToken = default)
//        {
//            bool hasMoreRecords = true;
//            while (hasMoreRecords && Data.IsCompleted == false)
//            {
//                ProjectionVersion version = null;
//                Type projectionType = version.ProjectionName.GetTypeByContract();

//                IndexStatus indexStatus = GetIndexStatus<EventToAggregateRootId>();
//                if (indexStatus.IsNotPresent() && IsNotSystemProjection(projectionType)) return ReplayResult.RetryLater($"The index is not present");

//                if (IsVersionTrackerMissing() && IsNotSystemProjection(projectionType)) return ReplayResult.RetryLater($"Projection `{version}` still don't have present index."); //WHEN TO RETRY AGAIN
//                if (HasReplayTimeout(Data.DueDate)) return ReplayResult.Timeout($"Rebuild of projection `{version}` has expired. Version:{version} Deadline:{Data.DueDate}.");

//                var allVersions = GetAllVersions(version);
//                if (allVersions.IsOutdatad(version)) return new ReplayResult($"Version `{version}` is outdated. There is a newer one which is already live.");
//                if (allVersions.IsCanceled(version)) return new ReplayResult($"Version `{version}` was canceled.");


//                var result = eventStorePlayer.LoadAggregateCommits(Data.PaginationToken);
//                foreach (var aggregateCommit in result.Commits)
//                {
//                    index.Index(aggregateCommit);
//                }

//                Data.PaginationToken = result.PaginationToken;
//                Data = await cluster.PingAsync(Data).ConfigureAwait(false);

//                hasMoreRecords = result.Commits.Any();
//            }

//            Data.IsCompleted = true;
//            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

//            return JobExecutionStatus.Completed;
//        }

//        IndexStatus GetIndexStatus<TIndex>() where TIndex : IEventStoreIndex
//        {
//            var id = new EventStoreIndexManagerId(typeof(TIndex).GetContractId(), context.Tenant);
//            var result = projectionReader.Get<EventStoreIndexStatus>(id);
//            if (result.IsSuccess)
//                return result.Data.State.Status;

//            return IndexStatus.NotPresent;
//        }

//        bool IsNotSystemProjection(Type projectionType)
//        {
//            return typeof(ISystemProjection).IsAssignableFrom(projectionType) == false;
//        }

//        bool IsVersionTrackerMissing()
//        {
//            var versionId = new ProjectionVersionManagerId(ProjectionVersionsHandler.ContractId, context.Tenant);
//            var result = projectionReader.Get<ProjectionVersionsHandler>(versionId);

//            return result.HasError || result.NotFound;
//        }

//        private bool HasReplayTimeout(DateTimeOffset replayUntil)
//        {
//            return DateTimeOffset.UtcNow >= replayUntil;
//        }

//        ProjectionVersions GetAllVersions(ProjectionVersion version)
//        {
//            var versionId = new ProjectionVersionManagerId(version.ProjectionName, context.Tenant);
//            var result = projectionReader.Get<ProjectionVersionsHandler>(versionId);
//            if (result.IsSuccess)
//                return result.Data.State.AllVersions;

//            return new ProjectionVersions();
//        }

//        private bool CanIndex()
//        {
//            return false;
//        }

//        public void SetTimeBox(VersionRequestTimebox timebox)
//        {
//            var dataOverride = BuildInitialData();
//            dataOverride.Timestamp = timebox.RebuildStartAt;
//            dataOverride.DueDate = timebox.RebuildFinishUntil;

//            OverrideData(fromCluster => Override(fromCluster, dataOverride));
//        }

//        private RebuildProjectionIndex_JobData Override(RebuildProjectionIndex_JobData fromCluster, RebuildProjectionIndex_JobData dataOverride)
//        {
//            if (fromCluster.IsCompleted && fromCluster.Timestamp < dataOverride.Timestamp)
//                return dataOverride;
//            else
//                return fromCluster;
//        }
//    }

//    public class RebuildIndex_ProjectionIndex_JobFactory
//    {
//        private readonly RebuildIndex_ProjectionIndex_Job job;

//        public RebuildIndex_ProjectionIndex_JobFactory(RebuildIndex_ProjectionIndex_Job job)
//        {
//            this.job = job;
//        }

//        public RebuildIndex_ProjectionIndex_Job CreateJob(VersionRequestTimebox timebox)
//        {
//            job.SetTimeBox(timebox);

//            return job;
//        }
//    }

//    public class RebuildProjectionIndex_JobData
//    {
//        public bool IsCompleted { get; set; } = false;

//        public string PaginationToken { get; set; } = string.Empty;

//        public ProjectionVersion Version { get; set; }

//        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

//        public DateTimeOffset DueDate { get; set; } = DateTimeOffset.MaxValue;
//    }
//}
