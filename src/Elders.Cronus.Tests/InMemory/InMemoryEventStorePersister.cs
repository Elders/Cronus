using Elders.Cronus.EventStore.Index;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.InMemory;

public class InMemoryEventStore : IEventStore
{
    private InMemoryEventStoreStorage eventStoreStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryEventStore"/> class.
    /// </summary>
    /// <param name="eventStoreStorage">The event store storage.</param>
    public InMemoryEventStore(InMemoryEventStoreStorage eventStoreStorage)
    {
        this.eventStoreStorage = eventStoreStorage;
    }

    /// <inheritdoc />
    public Task<EventStream> LoadAsync(IBlobId aggregateId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new EventStream(eventStoreStorage.Seek(aggregateId)));
    }

    /// <inheritdoc />
    public Task AppendAsync(AggregateCommit aggregateCommit, CancellationToken cancellationToken = default)
    {
        eventStoreStorage.Flush(aggregateCommit);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AppendAsync(AggregateEventRaw aggregateCommitRaw, CancellationToken cancellationToken = default)
    {
        return Task.FromException(new System.NotImplementedException());
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(AggregateEventRaw eventRaw, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public Task<LoadAggregateRawEventsWithPagingResult> LoadWithPagingAsync(IBlobId aggregateId, PagingOptions pagingOptions, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public Task<AggregateEventRaw> LoadAggregateEventRaw(IndexRecord indexRecord, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
