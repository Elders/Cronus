using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections.Rebuilding
{
    public sealed class RebuildProjectionSequentially_Job : CronusJob<RebuildProjectionSequentially_JobData>
    {
        private readonly IPublisher<ISystemSignal> signalPublisher;
        private readonly ISerializer serializer;
        private readonly ICronusContextAccessor contextAccessor;
        private readonly EventLookupInByteArray eventLookupInByteArray;
        private readonly IInitializableProjectionStore projectionStoreInitializer;
        private readonly IEventStorePlayer player;
        private readonly IProjectionWriter projectionWriter;
        private readonly ProgressTracker progressTracker;
        private readonly ProjectionVersionHelper projectionVersionHelper;
        private static readonly Action<ILogger, string, ulong, Exception> LogProjectionProgress =
            LoggerMessage.Define<string, ulong>(LogLevel.Information, CronusLogEvent.CronusJobOk, "Rebuild projection job progress for version {cronus_projection_version}: {counter}");

        private static readonly Action<ILogger, string, Exception> LogRebuildProjectionCanceled =
            LoggerMessage.Define<string>(LogLevel.Information, CronusLogEvent.CronusJobError, "The rebuild job for version {cronus_projection_version} was cancelled.");

        private static readonly Action<ILogger, string, ulong, Exception> LogRebuildProjectionCompleted =
            LoggerMessage.Define<string, ulong>(LogLevel.Information, CronusLogEvent.CronusJobOk, "The rebuild job for version {cronus_projection_version} has completed. Total events: {counter}");

        public RebuildProjectionSequentially_Job(
            IInitializableProjectionStore projectionStoreInitializer,
            IEventStorePlayer player,
            IProjectionWriter projectionWriter,
            ProgressTracker progressTracker,
            ProjectionVersionHelper projectionVersionHelper,
            IPublisher<ISystemSignal> signalPublisher,
            ISerializer serializer,
            ICronusContextAccessor contextAccessor,
            EventLookupInByteArray eventLookupInByteArray,
            ILogger<RebuildProjectionSequentially_Job> logger)
            : base(logger)
        {
            this.signalPublisher = signalPublisher;
            this.serializer = serializer;
            this.contextAccessor = contextAccessor;
            this.eventLookupInByteArray = eventLookupInByteArray;
            this.projectionStoreInitializer = projectionStoreInitializer;
            this.progressTracker = progressTracker;
            this.projectionVersionHelper = projectionVersionHelper;
            this.player = player;
            this.projectionWriter = projectionWriter;
        }

        public override string Name { get; set; } = typeof(ProjectionIndex).GetContractId();

        protected override async Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            if (Data.IsCompleted)
                return JobExecutionStatus.Completed;

            ProjectionVersion version = Data.Version;
            Type projectionType = version.ProjectionName.GetTypeByContract();

            await progressTracker.InitializeAsync(version).ConfigureAwait(false);

            if (await projectionVersionHelper.ShouldBeRetriedAsync(version).ConfigureAwait(false))
                return JobExecutionStatus.Running;

            if (await projectionVersionHelper.ShouldBeCanceledAsync(version, Data.DueDate).ConfigureAwait(false))
                return JobExecutionStatus.Failed;

            bool isStoreInitialized = await projectionStoreInitializer.InitializeAsync(version).ConfigureAwait(false);
            if (isStoreInitialized == false)
                return JobExecutionStatus.Running;

            var startSignal = progressTracker.GetProgressStartedSignal();
            signalPublisher.Publish(startSignal);

            List<string> projectionEventsContractIds = projectionVersionHelper.GetInvolvedEventTypes(projectionType).Select(x => x.GetContractId()).ToList();

            var projectionInstance = contextAccessor.CronusContext.ServiceProvider.GetRequiredService(projectionType);

            var pingSource = new CancellationTokenSource();
            CancellationToken ct = pingSource.Token;
            if (projectionInstance is IAmEventSourcedProjection eventSourcedProjection)
            {
                PlayerOperator playerOperator = new PlayerOperator()
                {
                    OnAggregateStreamLoadedAsync = async stream =>
                    {
                        foreach (string eventTypeContract in projectionEventsContractIds)
                        {
                            var eventsData = stream.Commits
                                .SelectMany(x => x.Events)
                                .Where(x => IsInterested(eventTypeContract, x.Data))
                                .Select(x => x.Data);

                            foreach (var eventRaw in eventsData)
                            {
                                IEvent @event = serializer.DeserializeFromBytes<IEvent>(eventRaw);
                                @event = @event.Unwrap();

                                Task projectionStoreTask = projectionWriter.SaveAsync(projectionType, @event, version);
                                Task replayTask = eventSourcedProjection.ReplayEventAsync(@event);

                                try
                                {
                                    await Task.WhenAll([projectionStoreTask, replayTask]).ConfigureAwait(false);
                                }
                                catch (Exception ex)
                                {
                                    if (projectionStoreTask.IsFaulted)
                                        logger.LogError(ex, "Failed to persist event!");

                                    if (replayTask.IsFaulted)
                                        logger.LogError(ex, "Failed to replay event!");

                                    continue;
                                }

                                progressTracker.TrackAndNotify(@event.GetType().GetContractId(), ct);
                            }
                        }
                    },
                    NotifyProgressAsync = async options =>
                    {
                        Data.ProcessedCount = progressTracker.GetTotalProcessedCount();
                        Data.PaginationToken = options.PaginationToken;
                        Data.MaxDegreeOfParallelism = options.MaxDegreeOfParallelism;
                        Data = await cluster.PingAsync(Data).ConfigureAwait(false);

                        LogProjectionProgress(logger, version.ToString(), progressTracker.GetTotalProcessedCount(), null);
                    }
                };

                if (Data.IsCanceled || cancellationToken.IsCancellationRequested || await projectionVersionHelper.ShouldBeCanceledAsync(version, Data.DueDate).ConfigureAwait(false))
                {
                    if (Data.IsCanceled == false)
                        await CancelJobAsync(cluster).ConfigureAwait(false);

                    LogRebuildProjectionCanceled(logger, version.ToString(), null);
                    return JobExecutionStatus.Canceled;
                }

                PlayerOptions opt = new PlayerOptions()
                {
                    PaginationToken = Data.PaginationToken,
                    After = Data.After,
                    Before = Data.Before ?? DateTimeOffset.UtcNow,
                    MaxDegreeOfParallelism = Data.MaxDegreeOfParallelism
                };

                await player.EnumerateEventStore(playerOperator, opt).ConfigureAwait(false);

                await eventSourcedProjection.OnReplayCompletedAsync().ConfigureAwait(false);
            }
            else
            {
                logger.LogWarning("The projection does not implement {interface}. Canceling...", nameof(IAmEventSourcedProjection));
                if (Data.IsCanceled == false)
                    await CancelJobAsync(cluster).ConfigureAwait(false);

                pingSource.Cancel();
                LogRebuildProjectionCanceled(logger, version.ToString(), null);
                return JobExecutionStatus.Canceled;
            }

            pingSource.Cancel();
            Data.IsCompleted = true;
            Data.Timestamp = DateTimeOffset.UtcNow;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            var finishSignal = progressTracker.GetProgressFinishedSignal();
            signalPublisher.Publish(finishSignal);

            LogRebuildProjectionCompleted(logger, version.ToString(), progressTracker.GetTotalProcessedCount(), null);

            return JobExecutionStatus.Completed;
        }

        protected override RebuildProjectionSequentially_JobData Override(RebuildProjectionSequentially_JobData fromCluster, RebuildProjectionSequentially_JobData fromLocal)
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

        private bool IsInterested(string eventTypeContract, byte[] data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(eventTypeContract);
            var eventSpan = bytes.AsSpan();
            return eventLookupInByteArray.HasEventId(data.AsSpan(), eventSpan);
        }
    }
}
