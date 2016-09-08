using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Middleware
{
    public class ExecutionChain<TContext>
    {
        Queue<AbstractMiddleware<TContext>> executionQueue;

        public ExecutionChain()
        {
            executionQueue = new Queue<AbstractMiddleware<TContext>>();
        }
        public void Append(AbstractMiddleware<TContext> next)
        {
            executionQueue.Enqueue(next);
        }

        public Queue<AbstractMiddleware<TContext>> GetExecutionQueue()
        {
            return new Queue<AbstractMiddleware<TContext>>(executionQueue);
        }

        public Stack<AbstractMiddleware<TContext>> GetExecutionStack()
        {
            return new Stack<AbstractMiddleware<TContext>>(executionQueue.Reverse());
        }
    }
}
