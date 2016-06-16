namespace Elders.Cronus.Middleware
{
    public interface IMiddleware<TContext>
    {
        void Run(TContext context);
    }

    public abstract class Middleware<TContext> : AbstractMiddleware<TContext>, IMiddleware<TContext>
    {
        protected override object AbstractRun(Execution<TContext> context)
        {
            Run(context);
            return null;
        }
        new public void Run(TContext context)
        {
            base.Run(context);
        }

        protected abstract void Run(Execution<TContext> context);
    }

    public interface IMiddleware<TContext, TResult>
    {
        TResult Run(TContext context);
    }

    public abstract class Middleware<TContext, TResult> : AbstractMiddleware<TContext>, IMiddleware<TContext, TResult>
    {
        protected override object AbstractRun(Execution<TContext> context)
        {
            return Run(new Execution<TContext, TResult>(context));
        }

        new public TResult Run(TContext context)
        {
            return (TResult)base.Run(context);
        }

        protected override Execution<TContext> CreateExecutionContext(TContext context)
        {
            return new Execution<TContext, TResult>(context);
        }

        protected abstract TResult Run(Execution<TContext, TResult> context);
    }
}