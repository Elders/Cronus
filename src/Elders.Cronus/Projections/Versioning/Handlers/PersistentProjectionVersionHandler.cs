using System.Runtime.Serialization;
using Elders.Cronus.Projections.Snapshotting;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "6fd97755-c98a-43b4-822d-268066900cf9")]
    public class PersistentProjectionVersionHandler : ProjectionDefinition<PersistentProjectionHandlerState, ProjectionVersionManagerId>, IAmNotSnapshotable,
        IEventHandler<ProjectionVersionRequested>,
        IEventHandler<NewProjectionVersionIsNowLive>
    {
        public PersistentProjectionVersionHandler()
        {
            Subscribe<ProjectionVersionRequested>(x => x.Id);
            Subscribe<NewProjectionVersionIsNowLive>(x => x.Id);
        }

        public void Handle(ProjectionVersionRequested @event)
        {
            State.Id = @event.Id;
            State.Building = @event.Version;
        }

        public void Handle(NewProjectionVersionIsNowLive @event)
        {
            State.Id = @event.Id;
            State.Building = null;
            State.Live = @event.ProjectionVersion;
        }
    }

    public class PersistentProjectionHandlerState
    {
        public ProjectionVersionManagerId Id { get; set; }

        public ProjectionVersion Live { get; set; }

        public ProjectionVersion Building { get; set; }
    }
}
