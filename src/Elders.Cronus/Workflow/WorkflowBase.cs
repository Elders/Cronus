using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.Workflow
{
    public abstract class WorkflowBase<TContext> : IWorkflow where TContext : class
    {
        protected ExecutionChain<TContext> ExecutionChain { get; set; }

        public WorkflowBase()
        {
            Name = GetType().Name;
            ExecutionChain = new ExecutionChain<TContext>();
        }

        public string Name { get; protected set; }

        protected abstract Task<object> AbstractRunAsync(Execution<TContext> execution);
        protected virtual Task OnRunCompletedAsync(Execution<TContext> execution)
        {
            return Task.CompletedTask;
        }

        public async Task<object> RunAsync(TContext context)
        {
            Execution<TContext> execution = CreateExecutionContext(context);
            try
            {
                return await InvokeChainAsync(execution).ConfigureAwait(false);
            }
            finally
            {
                await OnRunCompletedAsync(execution);
            }
        }

        /// <summary>
        /// Adds the next workflow in the execution chain.
        /// </summary>
        /// <param name="nextWorkflow"></param>
        public void Use(WorkflowBase<TContext> nextWorkflow)
        {
            if (nextWorkflow is null) throw new ArgumentNullException(nameof(nextWorkflow));

            ExecutionChain.Append(nextWorkflow);
        }

        /// <summary>
        /// Overrides the whole execution flow.
        /// </summary>
        /// <param name="nextWorkflow"></param>
        public void Override(WorkflowBase<TContext> nextWorkflow)
        {
            if (nextWorkflow is null) throw new ArgumentNullException(nameof(nextWorkflow));
            ExecutionChain.Override(nextWorkflow);
        }

        protected async Task<object> InvokeChainAsync(Execution<TContext> control)
        {
            var iterator = control as IEnumerator<WorkflowBase<TContext>>;
            control.Follow(ExecutionChain);
            var result = await this.AbstractRunAsync(control).ConfigureAwait(false);
            control.ExecutionResult(result);
            var stupidityFactor = 1;
            while (iterator.MoveNext())
            {
                result = await iterator.Current.InvokeChainAsync(control).ConfigureAwait(false);
                control.ExecutionResult(result);

                stupidityFactor++;
                if (stupidityFactor > 1000)
                    throw new InvalidOperationException("Stupidity factor over 1000");

            }
            return control.PreviousResult;
        }

        protected virtual Execution<TContext> CreateExecutionContext(TContext context)
        {
            return new Execution<TContext>(context);
        }
    }
}
