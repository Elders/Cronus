using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Workflow;

public abstract class WorkflowBase<TContext> : IWorkflow where TContext : class
{
    protected ExecutionChain<TContext> ExecutionChain { get; set; }

    public WorkflowBase()
    {
        Name = GetType().Name;
        ExecutionChain = new ExecutionChain<TContext>();
    }

    public string Name { get; protected set; }

    /// <summary>
    /// Implements the workflow-specific execution logic.
    /// </summary>
    /// <param name="execution">The current execution context for the workflow chain.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    protected abstract Task<object> AbstractRunAsync(Execution<TContext> execution, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hook invoked after the workflow's execution chain has finished. Override to perform clean-up work.
    /// </summary>
    /// <param name="execution">The execution context for the run that just finished.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    protected virtual Task OnRunCompletedAsync(Execution<TContext> execution, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Runs the workflow over the supplied context.
    /// </summary>
    /// <param name="context">The workflow's input context.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task<object> RunAsync(TContext context, CancellationToken cancellationToken = default)
    {
        Execution<TContext> execution = CreateExecutionContext(context);
        try
        {
            return await InvokeChainAsync(execution, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await OnRunCompletedAsync(execution, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Adds the next workflow in the execution chain.
    /// </summary>
    /// <param name="nextWorkflow"></param>
    public void Use(WorkflowBase<TContext> nextWorkflow)
    {
        if (nextWorkflow is null) throw new ArgumentNullException(nameof(nextWorkflow));

        ExecutionChain.Append(nextWorkflow);
    }

    /// <summary>
    /// Overrides the whole execution flow.
    /// </summary>
    /// <param name="nextWorkflow"></param>
    public void Override(WorkflowBase<TContext> nextWorkflow)
    {
        if (nextWorkflow is null) throw new ArgumentNullException(nameof(nextWorkflow));
        ExecutionChain.Override(nextWorkflow);
    }

    /// <summary>
    /// Invokes the current workflow's <see cref="AbstractRunAsync"/> and then walks the appended chain in order.
    /// </summary>
    /// <param name="control">The shared execution control object.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    protected async Task<object> InvokeChainAsync(Execution<TContext> control, CancellationToken cancellationToken = default)
    {
        var iterator = control as IEnumerator<WorkflowBase<TContext>>;
        control.Follow(ExecutionChain);
        var result = await this.AbstractRunAsync(control, cancellationToken).ConfigureAwait(false);
        control.ExecutionResult(result);
        var stupidityFactor = 1;
        while (iterator.MoveNext())
        {
            result = await iterator.Current.InvokeChainAsync(control, cancellationToken).ConfigureAwait(false);
            control.ExecutionResult(result);

            stupidityFactor++;
            if (stupidityFactor > 1000)
                throw new InvalidOperationException("Stupidity factor over 1000");

        }
        return control.PreviousResult;
    }

    protected virtual Execution<TContext> CreateExecutionContext(TContext context)
    {
        return new Execution<TContext>(context);
    }
}
