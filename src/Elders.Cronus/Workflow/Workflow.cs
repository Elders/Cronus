namespace Elders.Cronus.Workflow
{
    public abstract class Workflow<TContext> : WorkflowBase<TContext> where TContext : class
    {
        protected override object AbstractRun(Execution<TContext> execution)
        {
            Run(execution);
            return null;
        }
        new public void Run(TContext context)
        {
            base.Run(context);
        }

        protected abstract void Run(Execution<TContext> execution);
    }

    public abstract class Workflow<TContext, TResult> : WorkflowBase<TContext> where TContext : class
    {
        protected override object AbstractRun(Execution<TContext> execution)
        {
            return Run(new Execution<TContext, TResult>(execution));
        }

        new public TResult Run(TContext context)
        {
            return (TResult)base.Run(context);
        }

        protected override Execution<TContext> CreateExecutionContext(TContext context)
        {
            return new Execution<TContext, TResult>(context);
        }

        protected abstract TResult Run(Execution<TContext, TResult> execution);
    }
}
