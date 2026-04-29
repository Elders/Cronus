using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Workflow;

/// <summary>
/// Convenience workflow that does not return a value from its run.
/// </summary>
/// <typeparam name="TContext">The workflow's input context type.</typeparam>
public abstract class Workflow<TContext> : WorkflowBase<TContext> where TContext : class
{
    /// <inheritdoc />
    protected override async Task<object> AbstractRunAsync(Execution<TContext> execution, CancellationToken cancellationToken = default)
    {
        await RunAsync(execution, cancellationToken).ConfigureAwait(false);
        return default(object);
    }

    /// <summary>
    /// Implements the workflow-specific logic for the void-returning variant.
    /// </summary>
    /// <param name="execution">The execution context.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    protected abstract Task RunAsync(Execution<TContext> execution, CancellationToken cancellationToken = default);
}

/// <summary>
/// Convenience workflow that returns a typed result from its run.
/// </summary>
/// <typeparam name="TContext">The workflow's input context type.</typeparam>
/// <typeparam name="TResult">The workflow's result type.</typeparam>
public abstract class Workflow<TContext, TResult> : WorkflowBase<TContext> where TContext : class
{
    /// <inheritdoc />
    protected override async Task<object> AbstractRunAsync(Execution<TContext> execution, CancellationToken cancellationToken = default)
    {
        return await RunAsync(new Execution<TContext, TResult>(execution), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs the workflow over the supplied context and returns the typed result.
    /// </summary>
    /// <param name="context">The workflow's input context.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    new public async Task<TResult> RunAsync(TContext context, CancellationToken cancellationToken = default)
    {
        return (TResult)await base.RunAsync(context, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override Execution<TContext> CreateExecutionContext(TContext context)
    {
        return new Execution<TContext, TResult>(context);
    }

    /// <summary>
    /// Implements the workflow-specific logic for the result-returning variant.
    /// </summary>
    /// <param name="execution">The typed execution context.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    protected abstract Task<TResult> RunAsync(Execution<TContext, TResult> execution, CancellationToken cancellationToken = default);
}
