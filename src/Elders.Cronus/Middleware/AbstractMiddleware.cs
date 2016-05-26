using System;

namespace Elders.Cronus.Middleware
{
    public abstract class AbstractMiddleware<TContext>
    {
        private Func<Execution<TContext>, object> executionChain;

        public AbstractMiddleware()
        {
            executionChain = (execution) =>
            {
                object result = AbstractRun(execution);
                execution.ExecutionResult(result);
                if (execution.Next != null)
                {
                    var current = execution.Next;
                    result = current.InvokeChain(execution);
                    execution.ExecutionResult(result);
                }
                return result;
            };


        }

        protected abstract object AbstractRun(Execution<TContext> execution);

        public object Run(TContext context)
        {
            return InvokeChain(CreateExecutionContext(context));
        }

        protected virtual Execution<TContext> CreateExecutionContext(TContext context)
        {
            return new Execution<TContext>(context, null);
        }

        public void Use(AbstractMiddleware<TContext> nextMiddleware)
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

            executionChain = (execution) =>
            {
                var previousPrpcess = execution.Next;
                execution.Transfer(nextMiddleware);
                object result = lastExecutionChain(execution);
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


        protected object InvokeChain(Execution<TContext> control)
        {
            return executionChain(control);
        }
    }
}