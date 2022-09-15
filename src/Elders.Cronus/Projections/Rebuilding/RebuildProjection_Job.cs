using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore.Index;

namespace Elders.Cronus.Projections.Rebuilding
{
    public sealed class RebuildProjection_Job : CronusJob<RebuildProjection_JobData>
    {
        private readonly IPublisher<ISystemSignal> signalPublisher;
        private readonly IInitializableProjectionStore projectionStoreInitializer;
        private readonly IRebuildingProjectionRepository rebuildingRepository;
        private readonly EventToAggregateRootId eventToAggregateIndex;
        private readonly ProgressTracker progressTracker;
        private readonly ProjectionVersionHelper projectionVersionHelper;

        public RebuildProjection_Job(
            IInitializableProjectionStore projectionStoreInitializer,
            IRebuildingProjectionRepository rebuildingRepository,
            ProgressTracker progressTracker,
            ProjectionVersionHelper projectionVersionHelper,
            EventToAggregateRootId eventToAggregateIndex,
            IPublisher<ISystemSignal> signalPublisher,
            ILogger<RebuildProjection_Job> logger)
            : base(logger)
        {
            this.signalPublisher = signalPublisher;
            this.projectionStoreInitializer = projectionStoreInitializer;
            this.eventToAggregateIndex = eventToAggregateIndex;
            this.progressTracker = progressTracker;
            this.projectionVersionHelper = projectionVersionHelper;
            this.rebuildingRepository = rebuildingRepository;
        }

        public override string Name { get; set; } = typeof(ProjectionIndex).GetContractId();

        protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            ProjectionVersion version = Data.Version;
            Type projectionType = version.ProjectionName.GetTypeByContract();

            await progressTracker.InitializeAsync(version).ConfigureAwait(false);

            if (await projectionVersionHelper.ShouldBeRetriedAsync(version).ConfigureAwait(false))
                return JobExecutionStatus.Running;

            if (await projectionVersionHelper.ShouldBeCanceledAsync(version, Data.DueDate).ConfigureAwait(false))
                return JobExecutionStatus.Failed;

            await projectionStoreInitializer.InitializeAsync(version).ConfigureAwait(false);

            var startSignal = progressTracker.GetProgressStartedSignal();
            signalPublisher.Publish(startSignal);

            IEnumerable<Type> projectionHandledEventTypes = projectionVersionHelper.GetInvolvedEventTypes(projectionType);

            foreach (Type eventType in projectionHandledEventTypes)
            {
                string eventTypeId = eventType.GetContractId();

                bool hasMoreRecords = true;
                while (hasMoreRecords && Data.IsCompleted == false)
                {
                    if (Data.IsCanceled || cancellationToken.IsCancellationRequested || await projectionVersionHelper.ShouldBeCanceledAsync(version, Data.DueDate).ConfigureAwait(false))
                    {
                        if (Data.IsCanceled == false)
                            await CancelJobAsync(cluster).ConfigureAwait(false);

                        logger.Info(() => $"The job {version} has been cancelled.");
                        return JobExecutionStatus.Canceled;
                    }

                    hasMoreRecords = await RebuildEventsAsync(eventTypeId, cluster, cancellationToken).ConfigureAwait(false);
                }
            }

            Data.IsCompleted = true;
            Data.Timestamp = DateTimeOffset.UtcNow;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            var finishSignal = progressTracker.GetProgressFinishedSignal();
            signalPublisher.Publish(finishSignal);

            logger.Info(() => $"The job has been completed.");
            return JobExecutionStatus.Completed;
        }

        private async Task<bool> RebuildEventsAsync(string eventTypeId, IClusterOperations cluster, CancellationToken cancellationToken)
        {
            RebuildProjection_JobData.EventPaging paging = Data.EventTypePaging.Where(et => et.Type.Equals(eventTypeId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            string paginationToken = paging?.PaginationToken;
            LoadIndexRecordsResult indexRecordsResult = await eventToAggregateIndex.EnumerateRecordsAsync(eventTypeId, paginationToken).ConfigureAwait(false); // TODO: Think about cassandra exception here. Such exceptions MUST be handled in the concrete impl of the IndexStore

            await rebuildingRepository.SaveAggregateCommitsAsync(indexRecordsResult.Records, Data).ConfigureAwait(false);

            await NotifyClusterAsync(eventTypeId, indexRecordsResult.PaginationToken, cluster, cancellationToken).ConfigureAwait(false);

            var hasMoreRecords = indexRecordsResult.Records.Any();

            return hasMoreRecords;
        }

        private async Task NotifyClusterAsync(string eventTypeId, string paginationToken, IClusterOperations cluster, CancellationToken cancellationToken)
        {
            ulong totalProcessed = progressTracker.CountTotalProcessedEvents();
            var progress = new RebuildProjection_JobData.EventPaging(eventTypeId, paginationToken, totalProcessed, progressTracker.TotalEvents);

            Data.MarkEventTypeProgress(progress);
            Data.Timestamp = DateTimeOffset.UtcNow;
            Data = await cluster.PingAsync(Data, cancellationToken).ConfigureAwait(false);

            var progressSignal = progressTracker.GetProgressSignal();
            signalPublisher.Publish(progressSignal);
        }

        protected override RebuildProjection_JobData Override(RebuildProjection_JobData fromCluster, RebuildProjection_JobData fromLocal)
        {
            if (fromCluster.IsCompleted && fromCluster.Timestamp < fromLocal.Timestamp || fromCluster.Version < fromLocal.Version)
                return fromLocal;
            else
                return fromCluster;
        }
        private async Task CancelJobAsync(IClusterOperations cluster)
        {
            Data.IsCanceled = true;
            Data.Timestamp = DateTimeOffset.UtcNow;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            var finishSignal = progressTracker.GetProgressFinishedSignal();
            signalPublisher.Publish(finishSignal);
        }
    }
}
