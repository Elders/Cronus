using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "f92c320f-ef20-49aa-a8bc-c7085cc3a731")]
    public class InMemoryProjectionVersionHandler : ISystemProjection,
            IEventHandler<ProjectionVersionRequested>,
            IEventHandler<NewProjectionVersionIsNowLive>,
            IEventHandler<ProjectionVersionRequestCanceled>
    {
        public InMemoryProjectionVersionStore ProjectionVersionStore { get; set; }

        public void Handle(ProjectionVersionRequestCanceled @event)
        {
            ProjectionVersionStore.Cache(@event.ProjectionVersion);
        }

        public void Handle(ProjectionVersionRequested @event)
        {
            ProjectionVersionStore.Cache(@event.ProjectionVersion);
        }

        public void Handle(NewProjectionVersionIsNowLive @event)
        {
            ProjectionVersionStore.Cache(@event.ProjectionVersion);
        }
    }
}
