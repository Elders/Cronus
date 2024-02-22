using Elders.Cronus.EventStore.Index;
using System;
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

    /// <summary>
    /// Loads all the commits of an aggregate with the specified aggregate identifier.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <returns></returns>
    public Task<EventStream> LoadAsync(IBlobId aggregateId)
    {
        return Task.FromResult(new EventStream(eventStoreStorage.Seek(aggregateId)));
    }

    /// <summary>
    /// Persists the specified aggregate commit.
    /// </summary>
    /// <param name="aggregateCommit">The aggregate commit.</param>
    public Task AppendAsync(AggregateCommit aggregateCommit)
    {
        eventStoreStorage.Flush(aggregateCommit);

        return Task.CompletedTask;
    }

    public Task AppendAsync(AggregateEventRaw aggregateCommitRaw)
    {
        return Task.FromException(new System.NotImplementedException());
    }

    public Task<bool> DeleteAsync(AggregateEventRaw eventRaw)
    {
        throw new System.NotImplementedException();
    }
    public Task<LoadAggregateRawEventsWithPagingResult> LoadWithPagingAsync(IBlobId aggregateId, PagingOptions pagingOptions)
    {
        throw new System.NotImplementedException();
    }

    public Task<AggregateEventRaw> LoadAggregateEventRaw(IndexRecord indexRecord)
    {
        throw new NotImplementedException();
    }
}
