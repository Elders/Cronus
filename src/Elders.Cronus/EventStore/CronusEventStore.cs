using Elders.Cronus.EventStore.Index;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

public class CronusEventStore : IEventStore
{
    private readonly IEventStore eventStore;
    private readonly ILogger<CronusEventStore> logger;

    public CronusEventStore(IEventStore eventStore, ILogger<CronusEventStore> logger)
    {
        this.eventStore = eventStore;
        this.logger = logger;
    }

    public async Task AppendAsync(AggregateCommit aggregateCommit)
    {
        try
        {
            await eventStore.AppendAsync(aggregateCommit).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to append aggregate with ID = {cronus_arid}.", aggregateCommit.AggregateRootId)))
        {
            throw;
        }
    }

    public async Task AppendAsync(AggregateEventRaw aggregateEventRaw)
    {
        try
        {
            await eventStore.AppendAsync(aggregateEventRaw).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to append aggregate with ID = {cronus_arid}.", aggregateEventRaw.AggregateRootId)))
        {
            throw;
        }
    }

    public async Task<bool> DeleteAsync(AggregateEventRaw eventRaw)
    {
        try
        {
            return await eventStore.DeleteAsync(eventRaw).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to delete aggregate event with ID = {cronus_arid}.", eventRaw.AggregateRootId)))
        {
            throw;
        }
    }

    public async Task<AggregateEventRaw> LoadAggregateEventRaw(IndexRecord indexRecord)
    {
        try
        {
            return await eventStore.LoadAggregateEventRaw(indexRecord).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to load aggregate event raw with ID = {cronus_arid}.", indexRecord.AggregateRootId)))
        {
            throw;
        }
    }

    public async Task<EventStream> LoadAsync(IBlobId aggregateId)
    {
        try
        {
            return await eventStore.LoadAsync(aggregateId).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to load aggregate with ID = {cronus_arid}.", aggregateId)))
        {
            throw;
        }
    }

    public async Task<LoadAggregateRawEventsWithPagingResult> LoadWithPagingAsync(IBlobId aggregateId, PagingOptions pagingOptions)
    {
        try
        {
            return await eventStore.LoadWithPagingAsync(aggregateId, pagingOptions).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to load aggregate with ID = {cronus_arid} and Paging options {@pagingOptions}.", aggregateId, pagingOptions)))
        {
            throw;
        }
    }
}
