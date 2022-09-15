using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Logging;

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
        public async Task CompleteActionWithProgressSignalAsync(IEnumerable<Func<Task<string>>> actions)
        {
            int counter = 0;

            try
            {
                List<Task> tasks = new List<Task>();
                foreach (var action in actions)
                {
                    counter++;
                    tasks.Add(ExecuteAndTrackAsync(action));

                    if (tasks.Count > 100)
                    {
                        Task finished = await Task.WhenAny(tasks).ConfigureAwait(false);
                        tasks.Remove(finished);
                    }

                    if (counter % 100 == 0)
                    {
                        var progressSignalche = GetProgressSignal();
                        signalPublisher.Publish(progressSignalche);
                    }
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
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
    }
}
