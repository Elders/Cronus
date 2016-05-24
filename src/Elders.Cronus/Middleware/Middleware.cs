using System;

namespace Elders.Cronus.Middleware
{
    public interface IMiddleware<TContext>
    {
        void Invoke(TContext context);
    }

    public abstract class Middleware<TContext> : AbstractMiddleware<TContext>, IMiddleware<TContext>
    {
        protected override object AbstractInvoke(TContext context, MiddlewareExecution<TContext> middlewareControl)
        {
            Invoke(context, middlewareControl);
            return null;
        }
        new public void Invoke(TContext context)
        {
            base.Invoke(context);
        }

        protected abstract void Invoke(TContext context, MiddlewareExecution<TContext> middlewareControl);
    }

    public interface IMiddleware<TContext, TResult>
    {
        TResult Invoke(TContext context);
    }

    public abstract class Middleware<TContext, TResult> : AbstractMiddleware<TContext>, IMiddleware<TContext, TResult>
    {
        protected override object AbstractInvoke(TContext context, MiddlewareExecution<TContext> middlewareControl)
        {
            return Invoke(context, new MiddlewareExecution<TContext, TResult>(middlewareControl));
        }

        new public TResult Invoke(TContext context)
        {
            return (TResult)base.Invoke(context);
        }

        protected override MiddlewareExecution<TContext> CreateExecutionContext(TContext context)
        {
            return new MiddlewareExecution<TContext, TResult>(context, null);
        }

        protected abstract TResult Invoke(TContext context, MiddlewareExecution<TContext, TResult> middlewareControl);
    }
}