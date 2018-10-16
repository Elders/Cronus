using System;
using System.Collections.Generic;

namespace Elders.Cronus.Middleware
{
    public interface IMiddleware { }

    public abstract class AbstractMiddleware<TContext> : IMiddleware
    {
        protected ExecutionChain<TContext> ExecutionChain { get; set; }

        public AbstractMiddleware()
        {
            ExecutionChain = new ExecutionChain<TContext>();
        }

        protected abstract object AbstractRun(Execution<TContext> execution);

        public object Run(TContext context)
        {
            return InvokeChain(CreateExecutionContext(context));
        }

        /// <summary>
        /// Adds the next middleware in the execution chain.
        /// </summary>
        /// <param name="nextMiddleware"></param>
        public void Use(AbstractMiddleware<TContext> nextMiddleware)
        {
            if (ReferenceEquals(null, nextMiddleware)) throw new ArgumentNullException(nameof(nextMiddleware), "Who are you?");
            ExecutionChain.Append(nextMiddleware);
        }

        protected object InvokeChain(Execution<TContext> control)
        {
            var iterator = control as IEnumerator<AbstractMiddleware<TContext>>;
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
