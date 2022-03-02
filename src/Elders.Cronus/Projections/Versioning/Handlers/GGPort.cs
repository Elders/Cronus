using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning.Handlers
{
    [DataContract(Name = "49ff4195-0a8b-43f2-a55e-6e76a91d7bf0")]
    public class GGPort : ISystemPort,
        IEventHandler<NewProjectionVersionIsNowLive>
    {
        private readonly IProjectionStore projectionStore;
        private readonly CronusContext cronusContext;
        private readonly ILogger<GGPort> logger;

        public GGPort(IProjectionStore projectionStore, CronusContext cronusContext, ILogger<GGPort> logger)
        {
            this.projectionStore = projectionStore;
            this.cronusContext = cronusContext;
            this.logger = logger;
        }

        public void Handle(NewProjectionVersionIsNowLive @event)
        {
            var projectionType = @event.ProjectionVersion.ProjectionName.GetTypeByContract();
            if (projectionType.IsRebuildableProjection())
            {
                var id = Urn.Parse($"urn:cronus:{@event.ProjectionVersion.ProjectionName}");

                IAmEventSourcedProjection projection = cronusContext.ServiceProvider.GetRequiredService(projectionType) as IAmEventSourcedProjection;

                int retry = 0;
                while (retry < 3)
                {
                    try
                    {
                        IEnumerable<ProjectionCommit> commits = projectionStore.EnumerateProjection(@event.ProjectionVersion, id);
                        projection.ReplayEvents(commits.Select(x => x.Event));
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(ex, () => "Error while replaying projection.");
                        retry++;
                    }
                }
            }
        }
    }

    interface IRebuildableProjection
    {
        void Rebuild(IEnumerable<IEvent> events);
    }

    public static class GGExtensions
    {
        public static bool IsRebuildableProjection(this Type projectionType)
        {
            return
                typeof(IAmEventSourcedProjection).IsAssignableFrom(projectionType) &&
                typeof(IProjectionDefinition).IsAssignableFrom(projectionType) == false;
        }
    }
}
