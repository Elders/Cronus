using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

/// <summary>
/// No-op aggregate-commit interceptor that returns the supplied commit unchanged.
/// </summary>
public class EmptyAggregateTransformer : IAggregateCommitInterceptor
{
    /// <inheritdoc />
    public Task OnAppendAsync(AggregateCommit origin, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task<AggregateCommit> OnAppendingAsync(AggregateCommit origin, CancellationToken cancellationToken = default) => Task.FromResult(origin);
}
