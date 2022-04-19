using System;
using System.Threading.Tasks;

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
        public static Workflow<TContext> Use<TContext>(this Workflow<TContext> self, Func<Execution<TContext>, Task> action) where TContext : class
        {
            ActionWorkflow<TContext> workflow = action is null ? ActionWorkflow<TContext>.Empty : new ActionWorkflow<TContext>(action);

            self.Use(workflow);
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
        public static Workflow<TContext, TResult> Use<TContext, TResult>(this Workflow<TContext, TResult> self, Func<Execution<TContext>, Task<TResult>> action) where TContext : class
        {
            ActionWorkflow<TContext, TResult> workflow = action is null ? ActionWorkflow<TContext, TResult>.Empty : new ActionWorkflow<TContext, TResult>(action);

            self.Use(workflow);
            return self;
        }

        /// <summary>
        /// Creates a <see cref="ActionWorkflow{TContext}"/> out of the specified <see cref="Action"/>
        /// </summary>
        /// <typeparam name="TContext">The context</typeparam>
        /// <param name="action">The action of the flow</param>
        /// <returns></returns>
        public static Workflow<TContext> Lamda<TContext>(Func<Execution<TContext>, Task> action = null) where TContext : class
        {
            if (action is null)
                return ActionWorkflow<TContext>.Empty;

            return new ActionWorkflow<TContext>(action);
        }

        public static Workflow<TContext, TResult> Lambda<TContext, TResult>(Func<Execution<TContext>, Task<TResult>> action = null) where TContext : class
        {
            if (action is null)
                return ActionWorkflow<TContext, TResult>.Empty;

            return new ActionWorkflow<TContext, TResult>(action);
        }
    }
}
