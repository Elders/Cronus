using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Workflow
{
    public class ExecutionChain<TContext>
    {
        Queue<WorkflowBase<TContext>> executionQueue;

        public ExecutionChain()
        {
            executionQueue = new Queue<WorkflowBase<TContext>>();
        }

        public void Append(WorkflowBase<TContext> next)
        {
            executionQueue.Enqueue(next);
        }

        public void Override(WorkflowBase<TContext> next)
        {
            executionQueue.Clear();
            Append(next);
        }

        public Queue<WorkflowBase<TContext>> GetExecutionQueue()
        {
            return new Queue<WorkflowBase<TContext>>(executionQueue);
        }

        public Stack<WorkflowBase<TContext>> GetExecutionStack()
        {
            return new Stack<WorkflowBase<TContext>>(executionQueue.Reverse());
        }
    }
}
