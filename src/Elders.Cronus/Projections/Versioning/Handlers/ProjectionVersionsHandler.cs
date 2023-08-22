using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = ContractId)]
    public class ProjectionVersionsHandler : ProjectionDefinition<ProjectionVersionsHandlerState, ProjectionVersionManagerId>, ISystemProjection, INonVersionableProjection,
        IEventHandler<ProjectionVersionRequested>,
        IEventHandler<NewProjectionVersionIsNowLive>,
        IEventHandler<ProjectionVersionRequestCanceled>,
        IEventHandler<ProjectionVersionRequestTimedout>

    {
        public const string ContractId = "f1469a8e-9fc8-47f5-b057-d5394ed33b4c";

        public ProjectionVersionsHandler()
        {
            Subscribe<ProjectionVersionRequested>(x => x.Id);
            Subscribe<NewProjectionVersionIsNowLive>(x => x.Id);
            Subscribe<ProjectionVersionRequestCanceled>(x => x.Id);
            Subscribe<ProjectionVersionRequestTimedout>(x => x.Id);
        }

        public Task HandleAsync(ProjectionVersionRequested @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.Version);
            return Task.CompletedTask;
        }

        public Task HandleAsync(NewProjectionVersionIsNowLive @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.ProjectionVersion);
            return Task.CompletedTask;
        }

        public Task HandleAsync(ProjectionVersionRequestCanceled @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.Version);
            return Task.CompletedTask;
        }

        public Task HandleAsync(ProjectionVersionRequestTimedout @event)
        {
            State.Id = @event.Id;
            State.AllVersions.Add(@event.Version);
            return Task.CompletedTask;
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
