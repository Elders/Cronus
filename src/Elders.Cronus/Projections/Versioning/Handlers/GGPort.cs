using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elders.Cronus.Projections.Versioning.Handlers
{
    public class GGPort : IPort,
        IEventHandler<NewProjectionVersionIsNowLive>
    {
        private readonly IProjectionStore projectionStore;
        private readonly CronusContext cronusContext;

        public GGPort(IProjectionStore projectionStore, CronusContext cronusContext)
        {
            this.projectionStore = projectionStore;
            this.cronusContext = cronusContext;
        }

        public void Handle(NewProjectionVersionIsNowLive @event)
        {
            var projectionType = @event.ProjectionVersion.ProjectionName.GetTypeByContract();
            if (projectionType.IsRebuildableProjection())
            {
                var id = Urn.Parse($"urn:cronus:{@event.ProjectionVersion.ProjectionName}");
                ;

                IAmEventSourcedProjection projection = cronusContext.ServiceProvider.GetRequiredService(projectionType) as IAmEventSourcedProjection;
                var @events = projectionStore.EnumerateProjection(@event.ProjectionVersion, id);
                projection.ReplayEvents(@events.Select(x => x.Event));
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
