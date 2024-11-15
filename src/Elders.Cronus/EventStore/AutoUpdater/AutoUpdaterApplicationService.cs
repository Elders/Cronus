using Elders.Cronus.EventStore.AutoUpdater.Commands;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.AutoUpdater;

public class AutoUpdaterApplicationService : ApplicationService<AutoUpdater>, ISystemAppService,
    ICommandHandler<RequestAutoUpdate>,
    ICommandHandler<BulkRequestAutoUpdate>,
    ICommandHandler<FinishAutoUpdate>,
    ICommandHandler<FailAutoUpdate>
{
    public AutoUpdaterApplicationService(IAggregateRepository repository) : base(repository) { }

    public async Task HandleAsync(RequestAutoUpdate command)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id);
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

    public async Task HandleAsync(BulkRequestAutoUpdate command)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id);
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

    public async Task HandleAsync(FinishAutoUpdate command)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id);
        if (result.IsSuccess)
        {
            result.Data.FinishUpdate(command.Name);
            await repository.SaveAsync(result.Data).ConfigureAwait(false);
        }
    }

    public async Task HandleAsync(FailAutoUpdate command)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id);
        if (result.IsSuccess)
        {
            result.Data.FailUpdate(command.Name);
            await repository.SaveAsync(result.Data).ConfigureAwait(false);
        }
    }
}
