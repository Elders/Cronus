using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index;

[DataContract(Name = "7c414ffd-f5c6-48ba-9ae8-c0907f006560")]
public class EventStoreIndexManagerAppService : ApplicationService<EventStoreIndexManager>, ISystemAppService,
    ICommandHandler<RegisterIndex>,
    ICommandHandler<RebuildIndexCommand>,
    ICommandHandler<FinalizeEventStoreIndexRequest>
{
    public EventStoreIndexManagerAppService(IAggregateRepository repository) : base(repository) { }

    public async Task HandleAsync(RegisterIndex command)
    {
        EventStoreIndexManager ar = null;
        ReadResult<EventStoreIndexManager> result = await repository.LoadAsync<EventStoreIndexManager>(command.Id).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            ar = result.Data;
            ar.Register();
        }

        if (result.NotFound)
            ar = new EventStoreIndexManager(command.Id);

        await repository.SaveAsync(ar).ConfigureAwait(false);
    }

    public async Task HandleAsync(RebuildIndexCommand command)
    {
        EventStoreIndexManager ar = null;
        ReadResult<EventStoreIndexManager> result = await repository.LoadAsync<EventStoreIndexManager>(command.Id).ConfigureAwait(false);

        if (result.NotFound)
        {
            ar = new EventStoreIndexManager(command.Id);
        }

        if (result.IsSuccess)
        {
            ar = result.Data;
        }

        if (command.MaxDegreeOfParallelism.HasValue)
            ar.Rebuild(command.MaxDegreeOfParallelism.Value);
        else
            ar.Rebuild();

        await repository.SaveAsync(ar).ConfigureAwait(false);
    }

    public async Task HandleAsync(FinalizeEventStoreIndexRequest command)
    {
        EventStoreIndexManager ar = null;
        ReadResult<EventStoreIndexManager> result = await repository.LoadAsync<EventStoreIndexManager>(command.Id).ConfigureAwait(false);

        if (result.NotFound)
        {
            ar = new EventStoreIndexManager(command.Id);
        }

        if (result.IsSuccess)
        {
            ar = result.Data;
        }

        ar.FinalizeRequest();

        await repository.SaveAsync(ar).ConfigureAwait(false);
    }
}
