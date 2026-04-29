using Elders.Cronus.EventStore.Index;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

/// <summary>
/// Logging facade over the persistence-specific <see cref="IEventStore"/> implementation. Delegates work and writes structured error logs on failure.
/// </summary>
public class CronusEventStore : IEventStore
{
    private readonly IEventStore eventStore;
    private readonly ILogger<CronusEventStore> logger;

    public CronusEventStore(IEventStore eventStore, ILogger<CronusEventStore> logger)
    {
        this.eventStore = eventStore;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task AppendAsync(AggregateCommit aggregateCommit, CancellationToken cancellationToken = default)
    {
        try
        {
            await eventStore.AppendAsync(aggregateCommit, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to append aggregate with ID = {cronus_arid}.", aggregateCommit.AggregateRootId)))
        {
            throw;
        }
    }

    /// <inheritdoc />
    public async Task AppendAsync(AggregateEventRaw aggregateEventRaw, CancellationToken cancellationToken = default)
    {
        try
        {
            await eventStore.AppendAsync(aggregateEventRaw, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to append aggregate with ID = {cronus_arid}.", aggregateEventRaw.AggregateRootId)))
        {
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(AggregateEventRaw eventRaw, CancellationToken cancellationToken = default)
    {
        try
        {
            return await eventStore.DeleteAsync(eventRaw, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to delete aggregate event with ID = {cronus_arid}.", eventRaw.AggregateRootId)))
        {
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<AggregateEventRaw> LoadAggregateEventRaw(IndexRecord indexRecord, CancellationToken cancellationToken = default)
    {
        try
        {
            return await eventStore.LoadAggregateEventRaw(indexRecord, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to load aggregate event raw with ID = {cronus_arid}.", indexRecord.AggregateRootId)))
        {
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<EventStream> LoadAsync(IBlobId aggregateId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await eventStore.LoadAsync(aggregateId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to load aggregate with ID = {cronus_arid}.", aggregateId)))
        {
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<LoadAggregateRawEventsWithPagingResult> LoadWithPagingAsync(IBlobId aggregateId, PagingOptions pagingOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            return await eventStore.LoadWithPagingAsync(aggregateId, pagingOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (False(() => logger.LogError(ex, "Failed to load aggregate with ID = {cronus_arid} and Paging options {@pagingOptions}.", aggregateId, pagingOptions)))
        {
            throw;
        }
    }
}
