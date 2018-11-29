using System;

namespace Elders.Cronus.Workflow
{
    public static class WorkflowExtensions
    {
        /// <summary>
        /// Adds the next flow in the execution chain
        /// </summary>
        /// <typeparam name="TContext">The context</typeparam>
        /// <param name="self">The current flow</param>
        /// <param name="action">The action which will be executed after the current flow</param>
        /// <returns></returns>
        public static Workflow<TContext> Use<TContext>(this Workflow<TContext> self, Action<Execution<TContext>> action) where TContext : class
        {
            self.Use(new ActionWorkflow<TContext>(action));
            return self;
        }

        /// <summary>
        /// Adds the next flow in the execution chain
        /// </summary>
        /// <typeparam name="TContext">The context</typeparam>
        /// <typeparam name="TResult">The result</typeparam>
        /// <param name="self">The current flow</param>
        /// <param name="action">The action which will be executed after the current flow</param>
        /// <returns></returns>
        public static Workflow<TContext, TResult> Use<TContext, TResult>(this Workflow<TContext, TResult> self, Func<Execution<TContext>, TResult> action) where TContext : class
        {
            self.Use(new ActionWorkflow<TContext, TResult>(action));
            return self;
        }

        /// <summary>
        /// Creates a <see cref="ActionWorkflow{TContext}"/> out of the specified <see cref="Action"/>
        /// </summary>
        /// <typeparam name="TContext">The context</typeparam>
        /// <param name="action">The action of the flow</param>
        /// <returns></returns>
        public static Workflow<TContext> Lamda<TContext>(Action<Execution<TContext>> action = null) where TContext : class
        {
            return new ActionWorkflow<TContext>(action);
        }

        public static Workflow<TContext, TResult> Lambda<TContext, TResult>(Func<Execution<TContext>, TResult> action = null) where TContext : class
        {
            return new ActionWorkflow<TContext, TResult>(action);
        }
    }
}
