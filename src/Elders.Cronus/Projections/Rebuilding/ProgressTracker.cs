﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Projections.Rebuilding
{
    public class ProgressTracker
    {
        private readonly string tenant;
        private readonly IMessageCounter messageCounter;
        private readonly IPublisher<ISignal> signalPublisher;
        private readonly ProjectionVersionHelper projectionVersionHelper;
        public string ProjectionName { get; set; }
        public long Processed { get; set; }
        public long TotalEvents { get; set; }


        public ProgressTracker(IMessageCounter messageCounter, CronusContext context, IPublisher<ISignal> signalPublisher, ProjectionVersionHelper projectionVersionHelper)
        {
            tenant = context.Tenant;
            this.messageCounter = messageCounter;
            this.signalPublisher = signalPublisher;
            this.projectionVersionHelper = projectionVersionHelper;
        }

        /// <summary>
        /// Use Initialize for initializing progress for specified projection 
        /// </summary>
        /// <param name="version">Projection version that should be initialized</param>
        public void Initialize(ProjectionVersion version)
        {
            ProjectionName = version.ProjectionName;
            IEnumerable<Type> projectionHandledEventTypes = projectionVersionHelper.GetInvolvedEventTypes(ProjectionName.GetTypeByContract());
            TotalEvents = projectionHandledEventTypes.Select(type => messageCounter.GetCount(type)).Sum();
            Processed = 0;
        }

        /// <summary>
        /// Finishes the action and sending incrementing progress signal
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task CompleteActionWithProgressSignalAsync(Func<Task> action)
        {
            await action.Invoke().ConfigureAwait(false);

            if (++Processed % 100 == 0)
            {
                var progressSignalche = new RebuildProjectionProgress(tenant, ProjectionName, Processed, TotalEvents);
                signalPublisher.Publish(progressSignalche);
            }
        }

        public RebuildProjectionProgress GetProgressSignal()
        {
            return new RebuildProjectionProgress(tenant, ProjectionName, Processed, TotalEvents);
        }

        public RebuildProjectionFinished GetProgressFinishedSignal()
        {
            return new RebuildProjectionFinished(tenant, ProjectionName);
        }

        public RebuildProjectionStarted GetProgressStartedSignal()
        {
            return new RebuildProjectionStarted(tenant, ProjectionName);
        }
    }
}
