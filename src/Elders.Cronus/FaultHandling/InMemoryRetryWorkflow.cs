using System;
using System.Threading.Tasks;
using Elders.Cronus.FaultHandling.Strategies;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.FaultHandling;

public class InMemoryRetryWorkflow<TContext> : Workflow<TContext> where TContext : class
{
    private RetryPolicy retryPolicy;

    readonly Workflow<TContext> workflow;

    public InMemoryRetryWorkflow(Workflow<TContext> workflow, ILogger logger)
    {
        this.workflow = workflow;
        var retryStrategy = new Incremental(5, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(500));//Total 3 etries
        retryPolicy = new RetryPolicy(new TransientErrorCatchAllStrategy(), retryStrategy, logger);
    }

    protected override async Task RunAsync(Execution<TContext> execution)
    {
        if (execution is null) throw new ArgumentNullException(nameof(execution));

        await retryPolicy.ExecuteActionAsync(() => workflow.RunAsync(execution.Context));
    }
}
