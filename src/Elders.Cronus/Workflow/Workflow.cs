using System.Threading.Tasks;

namespace Elders.Cronus.Workflow
{
    public abstract class Workflow<TContext> : WorkflowBase<TContext> where TContext : class
    {
        protected override async Task<object> AbstractRunAsync(Execution<TContext> execution)
        {
            await RunAsync(execution).ConfigureAwait(false);
            return default(object);
        }

        protected abstract Task RunAsync(Execution<TContext> execution);
    }

    public abstract class Workflow<TContext, TResult> : WorkflowBase<TContext> where TContext : class
    {
        protected override async Task<object> AbstractRunAsync(Execution<TContext> execution)
        {
            return await RunAsync(new Execution<TContext, TResult>(execution)).ConfigureAwait(false);
        }

        new public async Task<TResult> RunAsync(TContext context)
        {
            return (TResult)await base.RunAsync(context);
        }

        protected override Execution<TContext> CreateExecutionContext(TContext context)
        {
            return new Execution<TContext, TResult>(context);
        }

        protected abstract Task<TResult> RunAsync(Execution<TContext, TResult> execution);
    }
}
