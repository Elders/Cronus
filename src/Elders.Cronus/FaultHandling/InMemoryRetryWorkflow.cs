using System;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.FaultHandling.Strategies;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.FaultHandling;

/// <summary>
/// Workflow decorator that retries the inner workflow in-memory using a transient-error retry policy.
/// </summary>
public class InMemoryRetryWorkflow<TContext> : Workflow<TContext> where TContext : class
{
    private RetryPolicy retryPolicy;

    readonly Workflow<TContext> workflow;

    public InMemoryRetryWorkflow(Workflow<TContext> workflow, ILogger logger)
    {
        this.workflow = workflow;
        var retryStrategy = new Incremental(5, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(500));//Total 3 etries
        retryPolicy = new RetryPolicy(new TransientErrorCatchAllStrategy(), retryStrategy, logger);
    }

    /// <inheritdoc />
    protected override async Task RunAsync(Execution<TContext> execution, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(execution);

        await retryPolicy.ExecuteActionAsync(ct => workflow.RunAsync(execution.Context, ct), cancellationToken).ConfigureAwait(false);
    }
}
