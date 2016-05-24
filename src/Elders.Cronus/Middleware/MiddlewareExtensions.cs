using System;

namespace Elders.Cronus.Middleware
{
    public static class MiddlewareExtensions
    {
        public static Middleware<TContext> Next<TContext>(this Middleware<TContext> self, Action<TContext, MiddlewareContext<TContext>> action)
        {
            self.Next(new SimpleMiddleware<TContext>(action));
            return self;
        }

        public static Middleware<TContext, TResult> Next<TContext, TResult>(this Middleware<TContext, TResult> self, Func<TContext, MiddlewareContext<TContext>, TResult> action)
        {
            self.Next(new SimpleMiddleware<TContext, TResult>(action));
            return self;
        }

        public static Middleware<TContext> Lamda<TContext>(Action<TContext, MiddlewareContext<TContext>> action = null)
        {
            return new SimpleMiddleware<TContext>(action);
        }

        public static Middleware<TContext, TResult> Lambda<TContext, TResult>(Func<TContext, MiddlewareContext<TContext>, TResult> action = null)
        {
            return new SimpleMiddleware<TContext, TResult>(action);
        }
    }

    public class SimpleMiddleware<TContext> : Middleware<TContext>
    {
        Action<TContext, MiddlewareContext<TContext>> implementation;

        public SimpleMiddleware(Action<TContext, MiddlewareContext<TContext>> action = null)
        {
            this.implementation = action;
        }
        protected override void Invoke(MiddlewareContext<TContext> middlewareControl)
        {
            if (implementation != null)
                implementation(middlewareControl.Context, middlewareControl);
        }
    }
    public class SimpleMiddleware<TContext, TResult> : Middleware<TContext, TResult>
    {
        Func<TContext, MiddlewareContext<TContext>, TResult> implementation;

        public SimpleMiddleware(Func<TContext, MiddlewareContext<TContext>, TResult> action = null)
        {
            this.implementation = action;
        }
        protected override TResult Invoke(MiddlewareContext<TContext, TResult> middlewareControl)
        {
            if (implementation != null)
                return implementation(middlewareControl.Context, middlewareControl);
            else
                return default(TResult);
        }
    }
}