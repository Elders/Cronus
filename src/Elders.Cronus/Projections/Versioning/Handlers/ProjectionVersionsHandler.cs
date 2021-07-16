using System.Runtime.Serialization;
using Elders.Cronus.Projections.Snapshotting;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = ContractId)]
    public class ProjectionVersionsHandler : ProjectionDefinition<ProjectionVersionsHandlerState, ProjectionVersionManagerId>, IAmNotSnapshotable, ISystemProjection, INonVersionableProjection,
        IEventHandler<ProjectionVersionRequestedForReplay>,
        IEventHandler<NewProjectionVersionIsNowLive>,
        IEventHandler<ProjectionVersionRequestCanceled>,
        IEventHandler<ProjectionVersionRequestTimedout>,
        IEventHandler<ProjectionVersionRequestedForRebuild>,
        IEventHandler<ProjectionVersionRebuildCanceled>,
        IEventHandler<ProjectionFinishedRebuilding>,
        IEventHandler<ProjectionVersionRebuildHasTimedout>

    {
        public const string ContractId = "f1469a8e-9fc8-47f5-b057-d5394ed33b4c";

        public ProjectionVersionsHandler()
        {
            Subscribe<ProjectionVersionRequestedForReplay>(x => x.Id);
            Subscribe<NewProjectionVersionIsNowLive>(x => x.Id);
            Subscribe<ProjectionVersionRequestCanceled>(x => x.Id);
            Subscribe<ProjectionVersionRequestTimedout>(x => x.Id);
            Subscribe<ProjectionVersionRequestedForRebuild>(x => x.Id);
            Subscribe<ProjectionVersionRebuildCanceled>(x => x.Id);
            Subscribe<ProjectionFinishedRebuilding>(x => x.Id);
            Subscribe<ProjectionVersionRebuildHasTimedout>(x => x.Id);
        }

        public void Handle(ProjectionVersionRequestedForReplay @event)
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

        public void Handle(ProjectionVersionRequestedForRebuild @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.Version);
        }

        public void Handle(ProjectionFinishedRebuilding @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.ProjectionVersion);
        }

        public void Handle(ProjectionVersionRebuildCanceled @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.ProjectionVersion);
        }

        public void Handle(ProjectionVersionRebuildHasTimedout @event)
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
