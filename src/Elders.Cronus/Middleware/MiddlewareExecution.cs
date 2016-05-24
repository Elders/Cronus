using System;

namespace Elders.Cronus.Middleware
{
    public class MiddlewareContext<TContext, TResult> : MiddlewareContext<TContext>
    {
        public MiddlewareContext(TContext context, AbstractMiddleware<TContext> next) : base(context, next) { }

        public MiddlewareContext(MiddlewareContext<TContext> control) : base(control) { }

        new public TResult PreviousResult { get { return (TResult)base.PreviousResult; } }
    }

    public class MiddlewareContext<TContext>
    {
        public AbstractMiddleware<TContext> Next { get; private set; }

        public TContext Context { get; private set; }

        public object PreviousResult { get; private set; }

        public MiddlewareContext(TContext context, AbstractMiddleware<TContext> next)
        {
            Next = next;
            Context = context;
        }

        public MiddlewareContext(MiddlewareContext<TContext> copy)
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