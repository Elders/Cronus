using System;
using System.Collections.Generic;

namespace Elders.Cronus.Workflow
{
    public abstract class WorkflowBase<TContext> : IWorkflow
    {
        protected ExecutionChain<TContext> ExecutionChain { get; set; }

        public WorkflowBase()
        {
            ExecutionChain = new ExecutionChain<TContext>();
        }

        protected abstract object AbstractRun(Execution<TContext> execution);

        public object Run(TContext context)
        {
            return InvokeChain(CreateExecutionContext(context));
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

        protected object InvokeChain(Execution<TContext> control)
        {
            var iterator = control as IEnumerator<WorkflowBase<TContext>>;
            control.Follow(ExecutionChain);
            var result = this.AbstractRun(control);
            control.ExecutionResult(result);
            var stupidityFactor = 1;
            while (iterator.MoveNext())
            {
                result = iterator.Current.InvokeChain(control);
                control.ExecutionResult(result);

                stupidityFactor++;
                if (stupidityFactor > 1000)
                    throw new InvalidOperationException("Stupidty factor over 9000");

            }
            return control.PreviousResult;
        }

        protected virtual Execution<TContext> CreateExecutionContext(TContext context)
        {
            return new Execution<TContext>(context);
        }
    }
}
