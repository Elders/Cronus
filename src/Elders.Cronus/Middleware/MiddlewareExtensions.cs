using System;

namespace Elders.Cronus.Middleware
{
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds the next middleware in the execution chain
        /// </summary>
        /// <typeparam name="TContext">The context</typeparam>
        /// <param name="self">The current middleware</param>
        /// <param name="action">The action which will be executed after the current middleware</param>
        /// <returns></returns>
        public static Middleware<TContext> Use<TContext>(this Middleware<TContext> self, Action<Execution<TContext>> action)
        {
            self.Use(new SimpleMiddleware<TContext>(action));
            return self;
        }

        /// <summary>
        /// Adds the next middleware in the execution chain
        /// </summary>
        /// <typeparam name="TContext">The context</typeparam>
        /// <typeparam name="TResult">The result</typeparam>
        /// <param name="self">The current middleware</param>
        /// <param name="action">The action which will be executed after the current middleware</param>
        /// <returns></returns>
        public static Middleware<TContext, TResult> Use<TContext, TResult>(this Middleware<TContext, TResult> self, Func<Execution<TContext>, TResult> action)
        {
            self.Use(new SimpleMiddleware<TContext, TResult>(action));
            return self;
        }

        /// <summary>
        /// Creates a <see cref="SimpleMiddleware{TContext}"/> out of the specified <see cref="Action"/>
        /// </summary>
        /// <typeparam name="TContext">The context</typeparam>
        /// <param name="action">The action of the middleware</param>
        /// <returns></returns>
        public static Middleware<TContext> Lamda<TContext>(Action<Execution<TContext>> action = null)
        {
            return new SimpleMiddleware<TContext>(action);
        }

        public static Middleware<TContext, TResult> Lambda<TContext, TResult>(Func<Execution<TContext>, TResult> action = null)
        {
            return new SimpleMiddleware<TContext, TResult>(action);
        }
    }
}
