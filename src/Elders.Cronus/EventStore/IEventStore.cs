using Elders.Cronus.EventStore.Index;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

/// <summary>
/// Persists and reads aggregate event streams.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends a complete aggregate commit (one or more events) to the event store.
    /// </summary>
    /// <param name="aggregateCommit">The aggregate commit to append.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task AppendAsync(AggregateCommit aggregateCommit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a single raw aggregate event to the event store.
    /// </summary>
    /// <param name="eventRaw">The raw event to append.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task AppendAsync(AggregateEventRaw eventRaw, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the full event stream for a given aggregate id.
    /// </summary>
    /// <param name="aggregateId">The aggregate id whose stream should be loaded.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task<EventStream> LoadAsync(IBlobId aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a single raw aggregate event from the event store.
    /// </summary>
    /// <param name="eventRaw">The raw event to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task<bool> DeleteAsync(AggregateEventRaw eventRaw, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a page of raw aggregate events for the specified aggregate id.
    /// </summary>
    /// <param name="aggregateId">The aggregate id whose events should be paged.</param>
    /// <param name="pagingOptions">Paging options describing page size and continuation.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task<LoadAggregateRawEventsWithPagingResult> LoadWithPagingAsync(IBlobId aggregateId, PagingOptions pagingOptions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a single raw aggregate event using its index record.
    /// </summary>
    /// <param name="indexRecord">The index record pointing to the event.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task<AggregateEventRaw> LoadAggregateEventRaw(IndexRecord indexRecord, CancellationToken cancellationToken = default);
}

public interface IEventStore<TSettings> : IEventStore
    where TSettings : class
{
}
