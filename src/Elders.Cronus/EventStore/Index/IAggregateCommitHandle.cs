using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index;

/// <summary>
/// Handles a single aggregate commit produced by the event store.
/// </summary>
/// <typeparam name="T">The aggregate commit type.</typeparam>
public interface IAggregateCommitHandle<in T>
    where T : AggregateCommit
{
    /// <summary>
    /// Handles the supplied aggregate commit.
    /// </summary>
    /// <param name="event">The aggregate commit to handle.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}
