using System;
using Elders.Cronus.FaultHandling.Strategies;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.FaultHandling
{
    public class InMemoryRetryWorkflow<TContext> : Workflow<TContext>
    {
        private RetryPolicy retryPolicy;

        readonly Workflow<TContext> workflow;

        public InMemoryRetryWorkflow(Workflow<TContext> workflow)
        {
            this.workflow = workflow;
            var retryStrategy = new Incremental(3, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(100));//Total 3 Retrues
            retryPolicy = new RetryPolicy(new TransientErrorCatchAllStrategy(), retryStrategy);
        }

        protected override void Run(Execution<TContext> execution)
        {
            retryPolicy.ExecuteAction(() =>
            {
                workflow.Run(execution.Context);
            });
        }
    }
}
