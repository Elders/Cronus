using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance.Helpers;

namespace Elders.Cronus.Projections.Rebuilding
{
    public class ProgressTracker
    {
        private readonly string tenant;
        private readonly IMessageCounter messageCounter;
        private readonly IPublisher<ISystemSignal> signalPublisher;
        private readonly ProjectionVersionHelper projectionVersionHelper;
        private readonly ILogger<ProgressTracker> logger;

        public string ProjectionName { get; set; }
        public Dictionary<string, ulong> EventTypeProcessed { get; set; }
        public ulong TotalEvents { get; set; }

        public ProgressTracker(IMessageCounter messageCounter, CronusContext context, IPublisher<ISystemSignal> signalPublisher, ProjectionVersionHelper projectionVersionHelper, ILogger<ProgressTracker> logger)
        {
            EventTypeProcessed = new Dictionary<string, ulong>();
            tenant = context.Tenant;
            this.messageCounter = messageCounter;
            this.signalPublisher = signalPublisher;
            this.projectionVersionHelper = projectionVersionHelper;
            this.logger = logger;
        }

        /// <summary>
        /// Use Initialize for initializing progress for specified projection 
        /// </summary>
        /// <param name="version">Projection version that should be initialized</param>
        public async Task InitializeAsync(ProjectionVersion version)
        {
            EventTypeProcessed = new Dictionary<string, ulong>();

            ProjectionName = version.ProjectionName;
            IEnumerable<Type> projectionHandledEventTypes = projectionVersionHelper.GetInvolvedEventTypes(ProjectionName.GetTypeByContract());
            foreach (var eventType in projectionHandledEventTypes)
            {
                TotalEvents += (ulong)await messageCounter.GetCountAsync(eventType).ConfigureAwait(false);
                EventTypeProcessed.Add(eventType.GetContractId(), 0);
            }
        }

        /// <summary>
        /// Finishes the action and sending incrementing progress signal
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task CompleteActionWithProgressSignalAsync(Func<Task<string>> action)
        {
            try
            {
                await ExecuteAndTrackAsync(action);

                var progressSignalche = GetProgressSignal();
                signalPublisher.Publish(progressSignalche);

            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Error when saving aggregate commit for projection {ProjectionName}")) { }
        }

        private async Task ExecuteAndTrackAsync(Func<Task<string>> taskToExecute)
        {
            try
            {
                string actionId = await taskToExecute().ConfigureAwait(false);

                EventTypeProcessed.TryGetValue(actionId, out ulong processed);
                EventTypeProcessed[actionId] = ++processed;

            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Error when saving aggregate commit for projection {ProjectionName}")) { }
        }

        /// <summary>
        /// Finishes the action and sending incrementing progress signal
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task CompleteActionWithProgressSignalAsync(IEnumerable<Func<Task<string>>> actions)
        {
            try
            {
                await Task.WhenAll(actions.Select(x=> ExecuteAndTrackAsync(x)).ToArray()).ConfigureAwait(false);

                var progressSignalche = GetProgressSignal();
                signalPublisher.Publish(progressSignalche);
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Error when saving aggregate commit for projection {ProjectionName}")) { }
        }

        public RebuildProjectionProgress GetProgressSignal()
        {
            return new RebuildProjectionProgress(tenant, ProjectionName, CountTotalProcessedEvents(), TotalEvents);
        }

        public RebuildProjectionFinished GetProgressFinishedSignal()
        {
            return new RebuildProjectionFinished(tenant, ProjectionName);
        }

        public RebuildProjectionStarted GetProgressStartedSignal()
        {
            return new RebuildProjectionStarted(tenant, ProjectionName);
        }

        public ulong CountTotalProcessedEvents()
        {
            ulong totalProcessed = 0;
            foreach (var typeProcessed in EventTypeProcessed)
            {
                totalProcessed += typeProcessed.Value;
            }
            return totalProcessed;
        }
    }
}
