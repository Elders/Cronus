using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Elders.Cronus.Cluster.Job;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.EventStore;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Elders.Cronus.Projections.Cassandra.EventSourcing;
using Elders.Cronus.Workflow;

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
        private static readonly Action<ILogger, string, uint, Exception> LogProjectionProgress =
            LoggerMessage.Define<string, uint>(LogLevel.Information, CronusLogEvent.CronusJobOk, "Rebuild projection job progress for version {cronus_projection_version}: {counter}");

        private static readonly Action<ILogger, string, Exception> LogRebuildProjectionCanceled =
            LoggerMessage.Define<string>(LogLevel.Information, CronusLogEvent.CronusJobError, "The rebuild job for version {cronus_projection_version} was cancelled.");

        private static readonly Action<ILogger, string, uint, Exception> LogRebuildProjectionCompleted =
            LoggerMessage.Define<string, uint>(LogLevel.Information, CronusLogEvent.CronusJobOk, "The rebuild job for version {cronus_projection_version} has completed. Total events: {counter}");

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

        public override string Name { get; set; } = typeof(ProjectionIndex).GetContractId();

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
                    progressTracker.EventTypeProcessed[item.Type] = item.ProcessedCount;
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

            IEnumerable<Type> projectionHandledEventTypes = projectionVersionHelper.GetInvolvedEventTypes(projectionType);
            ConcurrentDictionary<Type, IAmEventSourcedProjection> projectionInstancesToReplay = new ConcurrentDictionary<Type, IAmEventSourcedProjection>();

            var pingSource = new CancellationTokenSource();
            CancellationToken ct = pingSource.Token;

            uint counter = 0;
            PlayerOperator @operator = new PlayerOperator()
            {
                OnLoadAsync = async eventRaw =>
                {
                    counter++;

                    IEvent @event = serializer.DeserializeFromBytes<IEvent>(eventRaw.Data);
                    if (@event is null)
                        return;

                    @event = @event.Unwrap();
                    IAmEventSourcedProjection instance;
                    if (projectionInstancesToReplay.TryGetValue(projectionType, out instance) == false)
                    {
                        instance = contextAccessor.CronusContext.ServiceProvider.GetRequiredService(projectionType) as IAmEventSourcedProjection;
                        if (instance is null)
                            return;
                        projectionInstancesToReplay.TryAdd(projectionType, instance);
                    }

                    await projectionWriter
                        .SaveAsync(projectionType, @event, version)
                        .ContinueWith(t => { instance?.ReplayEventAsync(@event); })
                        .ConfigureAwait(false);

                    progressTracker.TrackAndNotify(@event.GetType().GetContractId(), ct);
                },
                NotifyProgressAsync = async options =>
                {
                    var progress = new RebuildProjection_JobData.EventPaging(options.EventTypeId, options.PaginationToken, options.After, options.Before, progressTracker.GetTotalProcessedCount(options.EventTypeId), 0);
                    if (Data.MarkEventTypeProgress(progress))
                    {
                        Data.After = options.After;
                        Data.Before = options.Before;
                        Data.MaxDegreeOfParallelism = options.MaxDegreeOfParallelism;
                        Data.Timestamp = DateTimeOffset.UtcNow;
                        Data = await cluster.PingAsync(Data);
                    }

                    LogProjectionProgress(logger, version.ToString(), counter, null);
                }
            };

            foreach (Type eventType in projectionHandledEventTypes)
            {
                string eventTypeId = eventType.GetContractId();

                if (Data.IsCanceled || cancellationToken.IsCancellationRequested || await projectionVersionHelper.ShouldBeCanceledAsync(version, Data.DueDate).ConfigureAwait(false))
                {
                    if (Data.IsCanceled == false)
                        await CancelJobAsync(cluster).ConfigureAwait(false);

                    LogRebuildProjectionCanceled(logger, version.ToString(), null);
                    return JobExecutionStatus.Canceled;
                }

                var found = Data.EventTypePaging.Where(upstream => upstream.Type.Equals(eventTypeId)).SingleOrDefault();
                PlayerOptions opt = new PlayerOptions()
                {
                    EventTypeId = eventTypeId,
                    PaginationToken = found?.PaginationToken,
                    After = Data.After,
                    Before = Data.Before ?? DateTimeOffset.UtcNow,
                    MaxDegreeOfParallelism = Data.MaxDegreeOfParallelism
                };

                await player.EnumerateEventStore(@operator, opt).ConfigureAwait(false);
            }

            IAmEventSourcedProjection instance;
            if (projectionInstancesToReplay.TryGetValue(projectionType, out instance) == false)
            {
                instance = contextAccessor.CronusContext.ServiceProvider.GetRequiredService(projectionType) as IAmEventSourcedProjection;
                if (instance is null)
                    return JobExecutionStatus.Completed;
                projectionInstancesToReplay.TryAdd(projectionType, instance);
            }

            await instance.OnReplayCompletedAsync().ConfigureAwait(false);

            pingSource.Cancel();
            Data.IsCompleted = true;
            Data.Timestamp = DateTimeOffset.UtcNow;
            Data = await cluster.PingAsync(Data).ConfigureAwait(false);

            var finishSignal = progressTracker.GetProgressFinishedSignal();
            signalPublisher.Publish(finishSignal);

            LogRebuildProjectionCompleted(logger, version.ToString(), counter, null);
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
