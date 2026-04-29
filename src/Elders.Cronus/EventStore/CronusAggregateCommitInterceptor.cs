using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

/// <summary>
/// Composite <see cref="IAggregateCommitInterceptor"/> that fans out to every registered interceptor in order.
/// </summary>
public sealed class CronusAggregateCommitInterceptor : IAggregateCommitInterceptor
{
    private readonly IEnumerable<IAggregateCommitInterceptor> interceptors;

    public CronusAggregateCommitInterceptor(IEnumerable<IAggregateCommitInterceptor> interceptor)
    {
        this.interceptors = interceptor;
    }

    /// <inheritdoc />
    public async Task OnAppendAsync(AggregateCommit origin, CancellationToken cancellationToken = default)
    {
        foreach (var interceptor in interceptors)
        {
            await interceptor.OnAppendAsync(origin, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<AggregateCommit> OnAppendingAsync(AggregateCommit origin, CancellationToken cancellationToken = default)
    {
        AggregateCommit transformedCommit = new AggregateCommit(origin);

        foreach (var interceptor in interceptors)
        {
            transformedCommit = await interceptor.OnAppendingAsync(transformedCommit, cancellationToken).ConfigureAwait(false);
        }

        return transformedCommit;
    }
}
