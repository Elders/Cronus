using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Versioning;

[DataContract(Name = ContractId)]
public class ProjectionVersionsHandler : ProjectionDefinition<ProjectionVersionsHandlerState, ProjectionVersionManagerId>, ISystemProjection, INonVersionableProjection,
    IEventHandler<ProjectionVersionRequested>,
    IEventHandler<NewProjectionVersionIsNowLive>,
    IEventHandler<ProjectionVersionRequestCanceled>,
    IEventHandler<ProjectionVersionRequestTimedout>,
    IEventHandler<ProjectionVersionRequestPaused>
{
    public const string ContractId = "f1469a8e-9fc8-47f5-b057-d5394ed33b4c";

    public ProjectionVersionsHandler()
    {
        Subscribe<ProjectionVersionRequested>(x => x.Id);
        Subscribe<NewProjectionVersionIsNowLive>(x => x.Id);
        Subscribe<ProjectionVersionRequestCanceled>(x => x.Id);
        Subscribe<ProjectionVersionRequestTimedout>(x => x.Id);
        Subscribe<ProjectionVersionRequestPaused>(x => x.Id);
    }

    /// <summary>
    /// Records a newly requested projection version into the version history.
    /// </summary>
    /// <param name="event">The event signalling that a new projection version was requested.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(ProjectionVersionRequested @event, CancellationToken cancellationToken = default)
    {
        State.Id = @event.Id;
        State.AllVersions.Add(@event.Version);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records a newly live projection version into the version history.
    /// </summary>
    /// <param name="event">The event signalling that the projection version is now live.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(NewProjectionVersionIsNowLive @event, CancellationToken cancellationToken = default)
    {
        State.Id = @event.Id;
        State.AllVersions.Add(@event.ProjectionVersion);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records a cancelled projection version request.
    /// </summary>
    /// <param name="event">The event signalling that the version request was cancelled.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(ProjectionVersionRequestCanceled @event, CancellationToken cancellationToken = default)
    {
        State.Id = @event.Id;
        State.AllVersions.Add(@event.Version);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records a projection version request that has timed out.
    /// </summary>
    /// <param name="event">The event signalling that the version request timed out.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(ProjectionVersionRequestTimedout @event, CancellationToken cancellationToken = default)
    {
        State.Id = @event.Id;
        State.AllVersions.Add(@event.Version);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records a projection version request that has been paused.
    /// </summary>
    /// <param name="event">The event signalling that the version request was paused.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(ProjectionVersionRequestPaused @event, CancellationToken cancellationToken = default)
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
