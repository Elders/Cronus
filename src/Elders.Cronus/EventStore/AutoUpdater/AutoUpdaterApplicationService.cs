using Elders.Cronus.EventStore.AutoUpdater.Commands;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.AutoUpdater;

public class AutoUpdaterApplicationService : ApplicationService<AutoUpdater>,
    ICommandHandler<RequestAutoUpdate>,
    ICommandHandler<FinishAutoUpdate>,
    ICommandHandler<FailAutoUpdate>
{
    public AutoUpdaterApplicationService(IAggregateRepository repository) : base(repository) { } // TODO: decide via a strategy which ES to load from (if there are diferences in the table struture)

    public async Task HandleAsync(RequestAutoUpdate command)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id);
        if (result.NotFound)
        {
            AutoUpdater ar = new AutoUpdater(command.Id, command.MajorVersion, command.BoundedContext, command.Timestamp);
            await repository.SaveAsync(ar);
        }
        else if (result.IsSuccess)
        {
            result.Data.TriggerUpdate(command.MajorVersion, command.Timestamp);
            await repository.SaveAsync(result.Data);
        }
    }

    public async Task HandleAsync(FinishAutoUpdate command)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id);
        if (result.IsSuccess)
        {
            result.Data.FinishUpdate(command.Timestamp);
            await repository.SaveAsync(result.Data);
        }
    }

    public async Task HandleAsync(FailAutoUpdate command)
    {
        var result = await repository.LoadAsync<AutoUpdater>(command.Id);
        if (result.IsSuccess)
        {
            result.Data.FailUpdate(command.Timestamp);
            await repository.SaveAsync(result.Data);
        }
    }
}
