using System;

namespace Elders.Cronus.Middleware
{
    public static class MiddlewareExtensions
    {
        public static Middleware<TContext> Next<TContext>(this Middleware<TContext> self, Action<TContext, MiddlewareExecution<TContext>> action)
        {
            self.Next(new SimpleMiddleware<TContext>(action));
            return self;
        }

        public static Middleware<TContext, TResult> Next<TContext, TResult>(this Middleware<TContext, TResult> self, Func<TContext, MiddlewareExecution<TContext>, TResult> action)
        {
            self.Next(new SimpleMiddleware<TContext, TResult>(action));
            return self;
        }

        public static Middleware<TContext> Lamda<TContext>(Action<TContext, MiddlewareExecution<TContext>> action = null)
        {
            return new SimpleMiddleware<TContext>(action);
        }

        public static Middleware<TContext, TResult> Lambda<TContext, TResult>(Func<TContext, MiddlewareExecution<TContext>, TResult> action = null)
        {
            return new SimpleMiddleware<TContext, TResult>(action);
        }
    }

    public class SimpleMiddleware<TContext> : Middleware<TContext>
    {
        Action<TContext, MiddlewareExecution<TContext>> implementation;

        public SimpleMiddleware(Action<TContext, MiddlewareExecution<TContext>> action = null)
        {
            this.implementation = action;
        }
        protected override void Invoke(TContext context, MiddlewareExecution<TContext> middlewareControl)
        {
            if (implementation != null)
                implementation(context, middlewareControl);
        }
    }
    public class SimpleMiddleware<TContext, TResult> : Middleware<TContext, TResult>
    {
        Func<TContext, MiddlewareExecution<TContext>, TResult> implementation;

        public SimpleMiddleware(Func<TContext, MiddlewareExecution<TContext>, TResult> action = null)
        {
            this.implementation = action;
        }
        protected override TResult Invoke(TContext context, MiddlewareExecution<TContext, TResult> middlewareControl)
        {
            if (implementation != null)
                return implementation(context, middlewareControl);
            else
                return default(TResult);
        }
    }
}