using Elders.Cronus.EventStore.AutoUpdater.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.AutoUpdater;

/// <summary>
/// Application service that handles auto-update commands by loading the matching <see cref="AutoUpdater"/> aggregate and persisting the resulting changes.
/// </summary>
public class AutoUpdaterApplicationService : ApplicationService<AutoUpdater>, ISystemAppService,
    ICommandHandler<RequestAutoUpdate>,
    ICommandHandler<BulkRequestAutoUpdate>,
    ICommandHandler<FinishAutoUpdate>,
    ICommandHandler<FailAutoUpdate>
{
    public AutoUpdaterApplicationService(IAggregateRepository repository) : base(repository) { }

    /// <summary>
    /// Handles the <see cref="RequestAutoUpdate"/> command by creating or updating an <see cref="AutoUpdater"/> aggregate.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(RequestAutoUpdate command, CancellationToken cancellationToken = default)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id).ConfigureAwait(false);
        if (result.NotFound)
        {
            AutoUpdater ar = new AutoUpdater(command.Id, command.BoundedContext);
            ar.RequestUpdate(command.AutoUpdate.Name, command.AutoUpdate.Sequence, command.AutoUpdate.IsSystem);
            await repository.SaveAsync(ar).ConfigureAwait(false);
        }
        else if (result.IsSuccess)
        {
            result.Data.RequestUpdate(command.AutoUpdate.Name, command.AutoUpdate.Sequence, command.AutoUpdate.IsSystem);
            await repository.SaveAsync(result.Data).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles the <see cref="BulkRequestAutoUpdate"/> command by creating or updating an <see cref="AutoUpdater"/> aggregate.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(BulkRequestAutoUpdate command, CancellationToken cancellationToken = default)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id).ConfigureAwait(false);
        if (result.NotFound)
        {
            AutoUpdater ar = new AutoUpdater(command.Id, command.BoundedContext);
            ar.BulkRequestUpdate(command.AutoUpdates);
            await repository.SaveAsync(ar).ConfigureAwait(false);
        }
        else if (result.IsSuccess)
        {
            result.Data.BulkRequestUpdate(command.AutoUpdates);
            await repository.SaveAsync(result.Data).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles the <see cref="FinishAutoUpdate"/> command by marking the matching auto-updater as finished.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(FinishAutoUpdate command, CancellationToken cancellationToken = default)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            result.Data.FinishUpdate(command.Name);
            await repository.SaveAsync(result.Data).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles the <see cref="FailAutoUpdate"/> command by marking the matching auto-updater as failed.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(FailAutoUpdate command, CancellationToken cancellationToken = default)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            result.Data.FailUpdate(command.Name);
            await repository.SaveAsync(result.Data).ConfigureAwait(false);
        }
    }
}
