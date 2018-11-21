﻿using System;
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
            var retryStrategy = new Incremental(3, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(100));//Total 3 Retrues
            retryPolicy = new RetryPolicy(new TransientErrorCatchAllStrategy(), retryStrategy);
        }

        protected override void Run(Execution<TContext> execution)
        {
            if (execution is null) throw new ArgumentNullException(nameof(execution));

            TContext context = execution.Context;
            retryPolicy.ExecuteAction(() =>
            {
                workflow.Run(execution.Context);
            });
        }
    }
}
