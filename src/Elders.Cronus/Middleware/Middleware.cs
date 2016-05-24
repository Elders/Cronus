using System;

namespace Elders.Cronus.Middleware
{
    public interface IMiddleware<TContext>
    {
        void Invoke(TContext context);
    }

    public abstract class Middleware<TContext> : AbstractMiddleware<TContext>, IMiddleware<TContext>
    {
        protected override object AbstractInvoke(MiddlewareContext<TContext> context)
        {
            Invoke(context);
            return null;
        }
        new public void Invoke(TContext context)
        {
            base.Invoke(context);
        }

        protected abstract void Invoke(MiddlewareContext<TContext> context);
    }

    public interface IMiddleware<TContext, TResult>
    {
        TResult Invoke(TContext context);
    }

    public abstract class Middleware<TContext, TResult> : AbstractMiddleware<TContext>, IMiddleware<TContext, TResult>
    {
        protected override object AbstractInvoke(MiddlewareContext<TContext> context)
        {
            return Invoke(new MiddlewareContext<TContext, TResult>(context));
        }

        new public TResult Invoke(TContext context)
        {
            return (TResult)base.Invoke(context);
        }

        protected override MiddlewareContext<TContext> CreateExecutionContext(TContext context)
        {
            return new MiddlewareContext<TContext, TResult>(context, null);
        }

        protected abstract TResult Invoke(MiddlewareContext<TContext, TResult> context);
    }
}