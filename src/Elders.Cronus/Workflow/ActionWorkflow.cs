using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Workflow;

/// <summary>
/// Workflow whose implementation is supplied as a delegate.
/// </summary>
public sealed class ActionWorkflow<TContext> : Workflow<TContext> where TContext : class
{
    Func<Execution<TContext>, Task> implementation;

    public ActionWorkflow(Func<Execution<TContext>, Task> action = null)
    {
        this.implementation = action;
    }

    /// <inheritdoc />
    protected override Task RunAsync(Execution<TContext> execution, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(execution);

        if (implementation is not null)
            return implementation(execution);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Workflow whose implementation is supplied as a delegate and which returns a typed result.
/// </summary>
public sealed class ActionWorkflow<TContext, TResult> : Workflow<TContext, TResult> where TContext : class
{
    Func<Execution<TContext>, Task<TResult>> implementation;

    public ActionWorkflow(Func<Execution<TContext>, Task<TResult>> action = null)
    {
        this.implementation = action;
    }

    /// <inheritdoc />
    protected override Task<TResult> RunAsync(Execution<TContext, TResult> execution, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(execution);

        if (implementation is not null)
            return implementation(execution);
        else
            return Task.FromResult(default(TResult));
    }
}
