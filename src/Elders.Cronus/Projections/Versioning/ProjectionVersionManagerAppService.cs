using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Versioning;

/// <summary>
/// Application service that handles projection-versioning commands for the projection version manager aggregate.
/// </summary>
[DataContract(Name = "28345d27-0ccf-48dc-88dc-2d10bed829cf")]
public class ProjectionVersionManagerAppService : ApplicationService<ProjectionVersionManager>, ISystemAppService,
    ICommandHandler<RegisterProjection>,
    ICommandHandler<NewProjectionVersion>,
    ICommandHandler<FinalizeProjectionVersionRequest>,
    ICommandHandler<CancelProjectionVersionRequest>,
    ICommandHandler<TimeoutProjectionVersionRequest>,
    ICommandHandler<FixProjectionVersion>,
    ICommandHandler<PauseProjectionVersion>
{
    private readonly IProjectionVersioningPolicy projectionVersioningPolicy;
    private readonly IProjectionReader projectionReader;

    public ProjectionVersionManagerAppService(IAggregateRepository repository, IProjectionVersioningPolicy projectionVersioningPolicy, IProjectionReader projectionReader) : base(repository)
    {
        this.projectionVersioningPolicy = projectionVersioningPolicy;
        this.projectionReader = projectionReader;
    }

    /// <summary>
    /// Registers a projection by either creating a new manager aggregate or notifying an existing one of the new hash.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(RegisterProjection command, CancellationToken cancellationToken = default)
    {
        ProjectionVersionManager ar = null;
        ReadResult<ProjectionVersionManager> result = await repository.LoadAsync<ProjectionVersionManager>(command.Id).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            ar = result.Data;
            ar.NotifyHash(command.Hash, projectionVersioningPolicy, command.ReplayEventsOptions);

            if (await ShouldRebuildMissingSystemProjectionsAsync(command.Id, projectionReader).ConfigureAwait(false))
            {
                ar.Rebuild(command.Hash, projectionVersioningPolicy, command.ReplayEventsOptions);
            }
        }

        if (result.NotFound)
            ar = new ProjectionVersionManager(command.Id, command.Hash);

        await repository.SaveAsync(ar).ConfigureAwait(false);
    }

    /// <summary>
    /// Triggers a replay of an existing projection version.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(NewProjectionVersion command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command.Id, ar => ar.Replay(command.Hash, projectionVersioningPolicy, command.ReplayEventsOptions), cancellationToken);
    }

    /// <summary>
    /// Triggers a rebuild of a projection version.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(FixProjectionVersion command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command.Id, ar => ar.Rebuild(command.Hash, projectionVersioningPolicy, command.ReplayEventsOptions), cancellationToken);
    }

    /// <summary>
    /// Finalises a projection version request once it has finished rebuilding successfully.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(FinalizeProjectionVersionRequest command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command.Id, ar => ar.FinalizeVersionRequest(command.Version), cancellationToken);
    }

    /// <summary>
    /// Cancels a projection version request.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(CancelProjectionVersionRequest command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command.Id, ar => ar.CancelVersionRequest(command.Version, command.Reason), cancellationToken);
    }

    /// <summary>
    /// Marks a projection version request as timed out.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(TimeoutProjectionVersionRequest command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command.Id, ar => ar.VersionRequestTimedout(command.Version, command.Timebox), cancellationToken);
    }

    /// <summary>
    /// Pauses an in-flight projection version request.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public Task HandleAsync(PauseProjectionVersion command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command.Id, ar => ar.PauseVersionRequest(command.Version), cancellationToken);
    }

    private async Task<bool> ShouldRebuildMissingSystemProjectionsAsync(ProjectionVersionManagerId projectionId, IProjectionReader projectionReader)
    {
        string projectionName = projectionId.Id;

        if (projectionName.IsProjectionVersionHandler() || projectionName.IsEventStoreIndexStatus())
        {
            ReadResult<ProjectionVersionsHandler> result = await projectionReader.GetAsync<ProjectionVersionsHandler>(projectionId).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                ProjectionVersions versions = result.Data.State.AllVersions;
                return versions.HasLiveVersion == false && versions.HasRebuildingVersion() == false;
            }
            return true;
        }

        return false;
    }
}
