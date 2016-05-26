using System;

namespace Elders.Cronus.Middleware
{
    public class Execution<TContext, TResult> : Execution<TContext>
    {
        public Execution(TContext context, AbstractMiddleware<TContext> next) : base(context, next) { }

        public Execution(Execution<TContext> control) : base(control) { }

        new public TResult PreviousResult { get { return (TResult)base.PreviousResult; } }
    }

    public class Execution<TContext>
    {
        public AbstractMiddleware<TContext> Next { get; private set; }

        public TContext Context { get; private set; }

        public object PreviousResult { get; private set; }

        public Execution(TContext context, AbstractMiddleware<TContext> next)
        {
            Next = next;
            Context = context;
        }

        public Execution(Execution<TContext> copy)
        {
            Next = copy.Next;
            Context = copy.Context;
            PreviousResult = copy.PreviousResult;
        }

        public void Break()
        {
            if (Next != null)
                System.Diagnostics.Trace.WriteLine($"Breaking middleware {Next}");
            Next = null;
        }

        public void Transfer(AbstractMiddleware<TContext> next)
        {
            Next = next;
        }


        public void ChangeContext(TContext newContext)
        {
            Context = newContext;
        }

        public void ExecutionResult(object result)
        {
            PreviousResult = result;
        }
    }
}