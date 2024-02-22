using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Workflow;

public class ExecutionChain<TContext> where TContext : class
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

    /// <summary>
    /// This would clear all workflows which are registered until now in the ExecutionChain and add the one you pass as the only one present.
    /// BEWARE! This does not override the current execution of the overflow. Only those which are registered in the ExecutionChain which are
    /// going to be executed after the workflow you are overriding. If you want to allow the possibility to override the whole workflow from
    /// somewhere else, you should nest him inside <see cref="ActionWorkflow{TContext}"/> first.
    /// </summary>
    /// <param name="next"></param>
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
