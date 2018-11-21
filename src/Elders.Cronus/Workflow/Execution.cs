using System;
using System.Collections;
using System.Collections.Generic;

namespace Elders.Cronus.Workflow
{
    public class Execution<TContext, TResult> : Execution<TContext> where TContext : class
    {
        public Execution(TContext context) : base(context) { }

        public Execution(Execution<TContext> control) : base(control) { }

        new public TResult PreviousResult { get { return (TResult)base.PreviousResult; } }
    }

    public class Execution<TContext> : IEnumerator<WorkflowBase<TContext>> where TContext : class
    {
        LinkedList<WorkflowBase<TContext>> executionQueue;

        WorkflowBase<TContext> current;

        public TContext Context { get; private set; }

        public object PreviousResult { get; private set; }

        public Execution(TContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            this.executionQueue = new LinkedList<WorkflowBase<TContext>>();
            Context = context;
        }

        protected Execution(Execution<TContext> copy)
        {
            Context = copy.Context;
            executionQueue = copy.executionQueue;
            PreviousResult = copy.PreviousResult;
        }

        public void Break()
        {
            executionQueue.Clear();
        }

        public void Transfer(WorkflowBase<TContext> next)
        {
            executionQueue.Clear();
            executionQueue.Enqueue(next);
        }

        public void Next(WorkflowBase<TContext> next)
        {
            executionQueue.Push(next);
        }

        public void Follow(ExecutionChain<TContext> chain)
        {
            executionQueue.PushMany(chain.GetExecutionQueue());
        }

        public void ChangeContext(TContext newContext)
        {
            Context = newContext;
        }

        public void ExecutionResult(object result)
        {
            PreviousResult = result;
        }

        WorkflowBase<TContext> IEnumerator<WorkflowBase<TContext>>.Current { get { return current; } }

        object IEnumerator.Current { get { return current; } }

        void IDisposable.Dispose() { }

        bool IEnumerator.MoveNext()
        {
            if (executionQueue.Count > 0)
            {
                current = executionQueue.Dequeue();
                return true;
            }
            else
                return false;
        }

        void IEnumerator.Reset()
        {
            executionQueue = new LinkedList<WorkflowBase<TContext>>();
        }
    }
}
