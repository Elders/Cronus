using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index;

/// <summary>
/// Persistence contract for an event-store index — the per-tenant lookup table that maps an
/// index record id to the originating aggregate / event metadata. Implementations live in the
/// persistence adapters (e.g. Cronus.Persistence.Cassandra).
/// </summary>
public interface IIndexStore
{
    /// <summary>
    /// Appends an index record to the store.
    /// </summary>
    /// <param name="indexRecord">The index record to append.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    Task ApendAsync(IndexRecord indexRecord, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an index record from the store.
    /// </summary>
    /// <param name="indexRecord">The index record to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteAsync(IndexRecord indexRecord, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams every index record matching the supplied id.
    /// </summary>
    /// <param name="indexRecordId">The index record id to look up.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous enumeration.</param>
    /// <returns>An asynchronous sequence of matching index records.</returns>
    IAsyncEnumerable<IndexRecord> GetAsync(string indexRecordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the number of index records associated with the supplied id.
    /// </summary>
    /// <param name="indexRecordId">The index record id to look up.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes with the matching record count.</returns>
    Task<long> GetCountAsync(string indexRecordId, CancellationToken cancellationToken = default);
}

public class LoadIndexRecordsResult
{
    public LoadIndexRecordsResult()
    {
        Records = new List<IndexRecord>();
    }

    public string PaginationToken { get; set; }

    public IEnumerable<IndexRecord> Records { get; set; }

    public static LoadIndexRecordsResult Empty(string paginationToken)
    {
        return new LoadIndexRecordsResult()
        {
            Records = Enumerable.Empty<IndexRecord>(),
            PaginationToken = paginationToken
        };
    }
}
