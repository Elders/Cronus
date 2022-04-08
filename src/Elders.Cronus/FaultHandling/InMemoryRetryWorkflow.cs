using System;
using System.Threading.Tasks;
using Elders.Cronus.FaultHandling.Strategies;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.FaultHandling
{
    public class InMemoryRetryWorkflow<TContext> : Workflow<TContext> where TContext : class
    {
        private RetryPolicy retryPolicy;

        readonly Workflow<TContext> workflow;

        public InMemoryRetryWorkflow(Workflow<TContext> workflow)
        {
            this.workflow = workflow;
            var retryStrategy = new Incremental(3, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));//Total 3 Retrues
            retryPolicy = new RetryPolicy(new TransientErrorCatchAllStrategy(), retryStrategy);
        }

        protected override async Task RunAsync(Execution<TContext> execution)
        {
            if (execution is null) throw new ArgumentNullException(nameof(execution));

            TContext context = execution.Context;
            await retryPolicy.ExecuteAction(() =>
            {
                return workflow.RunAsync(execution.Context).ConfigureAwait(false);
            });
        }
    }
}
