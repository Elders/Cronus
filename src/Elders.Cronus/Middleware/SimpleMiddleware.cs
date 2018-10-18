using System;

namespace Elders.Cronus.Middleware
{
    public class SimpleMiddleware<TContext> : Middleware<TContext>
    {
        Action<Execution<TContext>> implementation;

        public SimpleMiddleware(Action<Execution<TContext>> action = null)
        {
            this.implementation = action;
        }

        protected override void Run(Execution<TContext> execution)
        {
            if (implementation != null)
                implementation(execution);
        }
    }

    public class SimpleMiddleware<TContext, TResult> : Middleware<TContext, TResult>
    {
        Func<Execution<TContext>, TResult> implementation;

        public SimpleMiddleware(Func<Execution<TContext>, TResult> action = null)
        {
            this.implementation = action;
        }

        protected override TResult Run(Execution<TContext, TResult> execution)
        {
            if (implementation != null)
                return implementation(execution);
            else
                return default(TResult);
        }
    }
}
