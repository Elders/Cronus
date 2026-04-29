using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

/// <summary>
/// Allows callers to inspect and rewrite aggregate commits as they enter and leave the event store's append pipeline.
/// </summary>
public interface IAggregateCommitInterceptor
{
    /// <summary>
    /// Invoked before the aggregate commit is appended; implementations may return a transformed commit.
    /// </summary>
    /// <param name="origin">The original aggregate commit produced by the caller.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task<AggregateCommit> OnAppendingAsync(AggregateCommit origin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoked after the aggregate commit has been appended.
    /// </summary>
    /// <param name="origin">The aggregate commit that was appended.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task OnAppendAsync(AggregateCommit origin, CancellationToken cancellationToken = default);
}
