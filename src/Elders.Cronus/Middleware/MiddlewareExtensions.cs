using System;

namespace Elders.Cronus.Middleware
{
    public static class MiddlewareExtensions
    {
        public static Middleware<TContext> Next<TContext>(this Middleware<TContext> self, Action<TContext, Execution<TContext>> action)
        {
            self.Use(new SimpleMiddleware<TContext>(action));
            return self;
        }

        public static Middleware<TContext, TResult> Next<TContext, TResult>(this Middleware<TContext, TResult> self, Func<TContext, Execution<TContext>, TResult> action)
        {
            self.Use(new SimpleMiddleware<TContext, TResult>(action));
            return self;
        }

        public static Middleware<TContext> Lamda<TContext>(Action<TContext, Execution<TContext>> action = null)
        {
            return new SimpleMiddleware<TContext>(action);
        }

        public static Middleware<TContext, TResult> Lambda<TContext, TResult>(Func<TContext, Execution<TContext>, TResult> action = null)
        {
            return new SimpleMiddleware<TContext, TResult>(action);
        }
    }

    public class SimpleMiddleware<TContext> : Middleware<TContext>
    {
        Action<TContext, Execution<TContext>> implementation;

        public SimpleMiddleware(Action<TContext, Execution<TContext>> action = null)
        {
            this.implementation = action;
        }
        protected override void Run(Execution<TContext> middlewareControl)
        {
            if (implementation != null)
                implementation(middlewareControl.Context, middlewareControl);
        }
    }
    public class SimpleMiddleware<TContext, TResult> : Middleware<TContext, TResult>
    {
        Func<TContext, Execution<TContext>, TResult> implementation;

        public SimpleMiddleware(Func<TContext, Execution<TContext>, TResult> action = null)
        {
            this.implementation = action;
        }
        protected override TResult Run(Execution<TContext, TResult> middlewareControl)
        {
            if (implementation != null)
                return implementation(middlewareControl.Context, middlewareControl);
            else
                return default(TResult);
        }
    }
}