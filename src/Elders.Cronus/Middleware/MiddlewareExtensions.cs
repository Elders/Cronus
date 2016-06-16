using System;

namespace Elders.Cronus.Middleware
{
    public static class MiddlewareExtensions
    {
        public static Middleware<TContext> Use<TContext>(this Middleware<TContext> self, Action<Execution<TContext>> action)
        {
            self.Use(new SimpleMiddleware<TContext>(action));
            return self;
        }

        public static Middleware<TContext, TResult> Use<TContext, TResult>(this Middleware<TContext, TResult> self, Func<Execution<TContext>, TResult> action)
        {
            self.Use(new SimpleMiddleware<TContext, TResult>(action));
            return self;
        }

        public static Middleware<TContext> Lamda<TContext>(Action<Execution<TContext>> action = null)
        {
            return new SimpleMiddleware<TContext>(action);
        }

        public static Middleware<TContext, TResult> Lambda<TContext, TResult>(Func<Execution<TContext>, TResult> action = null)
        {
            return new SimpleMiddleware<TContext, TResult>(action);
        }
    }

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
