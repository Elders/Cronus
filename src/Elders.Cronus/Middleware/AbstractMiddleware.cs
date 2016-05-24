using System;

namespace Elders.Cronus.Middleware
{
    public abstract class AbstractMiddleware<TContext>
    {
        private Func<TContext, MiddlewareExecution<TContext>, object> executionChain;

        public AbstractMiddleware()
        {
            executionChain = (ctx, next) => AbstractInvoke(ctx, next);
        }

        protected abstract object AbstractInvoke(TContext context, MiddlewareExecution<TContext> middlewareControl);

        public object Invoke(TContext context)
        {
            return InvokeChain(CreateExecutionContext(context));
        }

        protected virtual MiddlewareExecution<TContext> CreateExecutionContext(TContext context)
        {
            return new MiddlewareExecution<TContext>(context, null);
        }

        public void Next(AbstractMiddleware<TContext> nextMiddleware)
        {
            ///THIS IS NEEDED BEAOUSE OF LAMBDAS STATIC COMPILATION 
            var lastExecutionChain = executionChain;
            ///DO NOT REFACTOR THIS LINE SEE -> Compilation Stuff
            #region Compilation Stuff
            /*
            public class Example
            {
                public void Main()
                {
                    var x = 5;
                    Action<int> action = y => Console.WriteLine(y);
                }
            }
            Native compiler output:

            public class Example
            {
                [CompilerGenerated]
                private static Action<int> CS$<>9__CachedAnonymousMethodDelegate1;
                public void Main()
                {
                    if (C.CS$<>9__CachedAnonymousMethodDelegate1 == null)
                    {
                        C.CS$<>9__CachedAnonymousMethodDelegate1 = new Action<int>(C.<M>b__0);
                    }
                    Action<int> arg_1D_0 = C.CS$<>9__CachedAnonymousMethodDelegate1;
                }
                [CompilerGenerated]
                private static void <M>b__0(int y)
                {
                    Console.WriteLine(y);
                }
            }
            */
            #endregion

            executionChain = (context, execution) =>
            {

                var previousPrpcess = execution.Next;
                execution.Transfer(nextMiddleware);
                object result = lastExecutionChain(context, execution);
                execution.ExecutionResult(result);
                if (execution.Next != null)
                {
                    var current = execution.Next;
                    execution.Transfer(previousPrpcess);
                    result = current.InvokeChain(execution);
                    execution.ExecutionResult(result);
                }
                return result;
            };

        }

        protected object InvokeChain(MiddlewareExecution<TContext> control)
        {
            return executionChain(control.Context, control);
        }
    }
}