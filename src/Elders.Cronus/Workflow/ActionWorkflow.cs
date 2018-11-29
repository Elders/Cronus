using System;

namespace Elders.Cronus.Workflow
{
    public class ActionWorkflow<TContext> : Workflow<TContext> where TContext : class
    {
        Action<Execution<TContext>> implementation;

        public ActionWorkflow(Action<Execution<TContext>> action = null)
        {
            this.implementation = action;
        }

        protected override void Run(Execution<TContext> execution)
        {
            if (execution is null) throw new ArgumentNullException(nameof(execution));

            if (implementation != null)
                implementation(execution);
        }
    }

    public class ActionWorkflow<TContext, TResult> : Workflow<TContext, TResult> where TContext : class
    {
        Func<Execution<TContext>, TResult> implementation;

        public ActionWorkflow(Func<Execution<TContext>, TResult> action = null)
        {
            this.implementation = action;
        }

        protected override TResult Run(Execution<TContext, TResult> execution)
        {
            if (execution is null) throw new ArgumentNullException(nameof(execution));

            if (implementation != null)
                return implementation(execution);
            else
                return default(TResult);
        }
    }
}
