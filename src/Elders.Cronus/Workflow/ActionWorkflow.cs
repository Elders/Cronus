using System;
using System.Threading.Tasks;

namespace Elders.Cronus.Workflow;

public class ActionWorkflow<TContext> : Workflow<TContext> where TContext : class
{
    Func<Execution<TContext>, Task> implementation;

    public ActionWorkflow(Func<Execution<TContext>, Task> action = null)
    {
        this.implementation = action;
    }

    protected override Task RunAsync(Execution<TContext> execution)
    {
        if (execution is null) throw new ArgumentNullException(nameof(execution));

        if (implementation != null)
            return implementation(execution);

        return Task.CompletedTask;
    }
}

public class ActionWorkflow<TContext, TResult> : Workflow<TContext, TResult> where TContext : class
{
    Func<Execution<TContext>, Task<TResult>> implementation;

    public ActionWorkflow(Func<Execution<TContext>, Task<TResult>> action = null)
    {
        this.implementation = action;
    }

    protected override Task<TResult> RunAsync(Execution<TContext, TResult> execution)
    {
        if (execution is null) throw new ArgumentNullException(nameof(execution));

        if (implementation != null)
            return implementation(execution);
        else
            return Task.FromResult(default(TResult));
    }
}
