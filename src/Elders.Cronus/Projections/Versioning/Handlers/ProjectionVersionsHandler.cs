using System.Runtime.Serialization;
using Elders.Cronus.Projections.Snapshotting;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = ContractId)]
    public class ProjectionVersionsHandler : ProjectionDefinition<ProjectionVersionsHandlerState, ProjectionVersionManagerId>, IAmNotSnapshotable, ISystemProjection, INonVersionableProjection,
        IEventHandler<ProjectionVersionRequested>,
        IEventHandler<NewProjectionVersionIsNowLive>,
        IEventHandler<ProjectionVersionRequestCanceled>,
        IEventHandler<ProjectionVersionRequestTimedout>

    {
        public const string ContractId = "ac8e4ba0-b351-4a91-8943-dee9465002a8";

        public ProjectionVersionsHandler()
        {
            Subscribe<ProjectionVersionRequested>(x => x.Id);
            Subscribe<NewProjectionVersionIsNowLive>(x => x.Id);
            Subscribe<ProjectionVersionRequestCanceled>(x => x.Id);
            Subscribe<ProjectionVersionRequestTimedout>(x => x.Id);
        }

        public void Handle(ProjectionVersionRequested @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.Version);
        }

        public void Handle(NewProjectionVersionIsNowLive @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.ProjectionVersion);
        }

        public void Handle(ProjectionVersionRequestCanceled @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.Version);
        }

        public void Handle(ProjectionVersionRequestTimedout @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.Version);
        }
    }

    public class ProjectionVersionsHandlerState
    {
        public ProjectionVersionsHandlerState()
        {
            AllVersions = new ProjectionVersions();
        }

        public ProjectionVersionManagerId Id { get; set; }

        public ProjectionVersion Live { get { return AllVersions.GetLive(); } }

        public ProjectionVersions AllVersions { get; set; }
    }
}
