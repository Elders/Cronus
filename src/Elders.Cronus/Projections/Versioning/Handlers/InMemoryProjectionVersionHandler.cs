using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "f92c320f-ef20-49aa-a8bc-c7085cc3a731")]
    public class InMemoryProjectionVersionHandler : ISystemProjection,
        IEventHandler<ProjectionVersionRequestedForReplay>,
        IEventHandler<NewProjectionVersionIsNowLive>,
        IEventHandler<ProjectionVersionRequestCanceled>
    {
        private readonly InMemoryProjectionVersionStore projectionVersionStore;

        public InMemoryProjectionVersionHandler(InMemoryProjectionVersionStore projectionVersionStore)
        {
            this.projectionVersionStore = projectionVersionStore ?? throw new ArgumentNullException(nameof(projectionVersionStore));
        }

        public void Handle(ProjectionVersionRequestCanceled @event)
        {
            projectionVersionStore.Cache(@event.Version);
        }

        public void Handle(ProjectionVersionRequestedForReplay @event)
        {
            projectionVersionStore.Cache(@event.Version);
        }

        public void Handle(NewProjectionVersionIsNowLive @event)
        {
            projectionVersionStore.Cache(@event.ProjectionVersion);
        }
    }
}
