using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections.Rebuilding
{
    public sealed class RebuildProjection_Job : CronusJob<RebuildProjection_JobData>
    {
        private readonly IPublisher<ISystemSignal> signalPublisher;
        private readonly ISerializer serializer;
        private readonly ICronusContextAccessor contextAccessor;
        private readonly IInitializableProjectionStore projectionStoreInitializer;
        private readonly IEventStorePlayer player;
        private readonly IProjectionWriter projectionWriter;
        private readonly ProgressTracker progressTracker;
        private readonly ProjectionVersionHelper projectionVersionHelper;
        private static readonly Action<ILogger, string, ulong, double, Exception> LogProjectionProgress =
            LoggerMessage.Define<string, ulong, double>(LogLevel.Information, CronusLogEvent.CronusJobOk, "Rebuild projection job progress for version {cronus_projection_version}: {counter}. Average speed: {speed} events/s.");

        private static readonly Action<ILogger, string, Exception> LogRebuildProjectionCanceled =
            LoggerMessage.Define<string>(LogLevel.Information, CronusLogEvent.CronusJobError, "The rebuild job for version {cronus_projection_version} was cancelled.");

        private static readonly Action<ILogger, string, double, ulong, double, Exception> LogRebuildProjectionCompleted =
            LoggerMessage.Define<string, double, ulong, double>(LogLevel.Information, CronusLogEvent.CronusJobOk, "The rebuild job for version {cronus_projection_version} has completed in {Elapsed:0.0000} ms. Total events: {counter}. Average speed: {speed} events/s.");

        public RebuildProjection_Job(
            IInitializableProjectionStore projectionStoreInitializer,
            IEventStorePlayer player,
            IProjectionWriter projectionWriter,
            ProgressTracker progressTracker,
            ProjectionVersionHelper projectionVersionHelper,
            IPublisher<ISystemSignal> signalPublisher,
            ISerializer serializer,
            ICronusContextAccessor contextAccessor,
            ILogger<RebuildProjection_Job> logger)
            : base(logger)
        {
            this.signalPublisher = signalPublisher;
            this.serializer = serializer;
            this.contextAccessor = contextAccessor;
            this.projectionStoreInitializer = projectionStoreInitializer;
            this.progressTracker = progressTracker;
            this.projectionVersionHelper = projectionVersionHelper;
            this.player = player;
            this.projectionWriter = projectionWriter;
        }

        public override string Name { get; set; } = nameof(RebuildProjection_Job);

        protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            if (Data.IsCompleted)
                return JobExecutionStatus.Completed;

            ProjectionVersion version = Data.Version;
            Type projectionType = version.ProjectionName.GetTypeByContract();

            await progressTracker.InitializeAsync(version).ConfigureAwait(false);
            foreach (var item in Data.EventTypePaging)
            {
                if (progressTracker.EventTypeProcessed.ContainsKey(item.Type))
                {
                    progressTracker.EventTypeProcessed[item.Type].Value = item.ProcessedCount;
                }
            }

            if (await projectionVersionHelper.ShouldBeRetriedAsync(version).ConfigureAwait(false))
                return JobExecutionStatus.Running;

            if (await projectionVersionHelper.ShouldBeCanceledAsync(version, Data.DueDate).ConfigureAwait(false))
                return JobExecutionStatus.Failed;

            bool isStoreInitialized = await projectionStoreInitializer.InitializeAsync(version).ConfigureAwait(false);
            if (isStoreInitialized == false)
                return JobExecutionStatus.Running;

            var startSignal = progressTracker.GetProgressStartedSignal();
            signalPublisher.Publish(startSignal);

            List<string> projectionHandledEventTypes = projectionVersionHelper.GetInvolvedEventTypes(projectionType).Select(x => x.GetContractId()).ToList();
            var projectionInstance = contextAccessor.CronusContext.ServiceProvider.GetRequiredService(projectionType) as IAmEventSourcedProjectionFast;

            var pingSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            progressTracker.MarkProcessStart();
            foreach (string eventTypeId in projectionHandledEventTypes)
            {
                if (Data.IsCanceled || cancellationToken.IsCancellationRequested || await projectionVersionHelper.ShouldBeCanceledAsync(version, Data.DueDate).ConfigureAwait(false))
                {
                    if (Data.IsCanceled == false)
                        await CancelJobAsync(cluster).ConfigureAwait(false);

                    LogRebuildProjectionCanceled(logger, version.ToString(), null);
                    return JobExecutionStatus.Canceled;
                }

                PlayerOperator playerOperator = new PlayerOperator()
                {
                    OnLoadAsync = async eventRaw =>
                    {
                        IEvent @event = serializer.DeserializeFromBytes<IEvent>(eventRaw.Data);
                        @event = @event.Unwrap();
                        if (@event is null)
                        {
                            logger.LogError("Failed to deserialize event from data {data}.", eventRaw.Data);
                            return;
                        }

                        await projectionWriter.SaveAsync(projectionType, @event, version).ConfigureAwait(false);
                        if (projectionInstance is not null)
                            await projectionInstance.ReplayEventAsync(@event).ConfigureAwait(false);

                        progressTracker.TrackAndNotify(@event.GetType().GetContractId(), pingSource.Token);
                    },
                    NotifyProgressAsync = async options =>
                    {
                        var totalCount = progressTracker.GetTotalProcessedCount();
                        var progress = new RebuildProjection_JobData.EventPaging(options.EventTypeId, options.PaginationToken, options.After, options.Before, progressTracker.GetTotalProcessedCount(options.EventTypeId), 0);
                        if (Data.MarkEventTypeProgress(progress))
                        {
                            Data.After = options.After;
                            Data.Before = options.Before;
                            Data.MaxDegreeOfParallelism = options.MaxDegreeOfParallelism;
                            Data.Timestamp = DateTimeOffset.UtcNow;
                            Data = await cluster.PingAsync(Data).ConfigureAwait(false);
                        }

                        var avgSpeed = progressTracker.GetProcessedPerSecond();
                        LogProjectionProgress(logger, version.ToString(), totalCount, avgSpeed, null);
                    }
                };

                var found = Data.EventTypePaging.Where(upstream => upstream.Type.Equals(eventTypeId)).SingleOrDefault();
                PlayerOptions opt = new PlayerOptions()
                {
                    EventTypeId = eventTypeId,
                    PaginationToken = found?.PaginationToken,
                    After = Data.After,
                    Before = Data.Before ?? DateTimeOffset.UtcNow,
                    MaxDegreeOfParallelism = Data.MaxDegreeOfParallelism
                };

                await player.EnumerateEventStore(playerOperator, opt, cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                //progressTracker.SayAnyFinalWords();
                return JobExecutionStatus.Canceled;
            }

            if (projectionInstance is not null)
                await projectionInstance.OnReplayCompletedAsync().ConfigureAwait(false);

            pingSource.Cancel();
            Data.IsCompleted = true;
            Data.Timestamp = DateTimeOffset.UtcNow;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            var finishSignal = progressTracker.GetProgressFinishedSignal();
            signalPublisher.Publish(finishSignal);

            var totalCount = progressTracker.GetTotalProcessedCount();
            var avgSpeed = progressTracker.GetProcessedPerSecond();

            LogRebuildProjectionCompleted(logger, version.ToString(), progressTracker.GetElapsed().TotalMilliseconds, totalCount, avgSpeed, null);

            return JobExecutionStatus.Completed;
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
