using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index;

/// <summary>
/// Application service that handles index-management commands for the event-store index aggregate.
/// </summary>
[DataContract(Name = "7c414ffd-f5c6-48ba-9ae8-c0907f006560")]
public class EventStoreIndexManagerAppService : ApplicationService<EventStoreIndexManager>, ISystemAppService,
    ICommandHandler<RegisterIndex>,
    ICommandHandler<RebuildIndexCommand>,
    ICommandHandler<FinalizeEventStoreIndexRequest>
{
    public EventStoreIndexManagerAppService(IAggregateRepository repository) : base(repository) { }

    /// <summary>
    /// Handles the <see cref="RegisterIndex"/> command by registering or creating the index aggregate.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(RegisterIndex command, CancellationToken cancellationToken = default)
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

    /// <summary>
    /// Handles the <see cref="RebuildIndexCommand"/> by triggering a rebuild on the index aggregate.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(RebuildIndexCommand command, CancellationToken cancellationToken = default)
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

    /// <summary>
    /// Handles the <see cref="FinalizeEventStoreIndexRequest"/> by marking the index request as finalized.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(FinalizeEventStoreIndexRequest command, CancellationToken cancellationToken = default)
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
