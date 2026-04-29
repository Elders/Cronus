using System;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Workflow;

/// <summary>
/// Workflow decorator that swallows any exception thrown by the inner workflow and logs it.
/// </summary>
public sealed class ExceptionEaterWorkflow<TContext> : Workflow<TContext> where TContext : HandleContext
{
    private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(DiagnosticsWorkflow<>));

    readonly Workflow<TContext> workflow;

    public ExceptionEaterWorkflow(Workflow<TContext> workflow)
    {
        this.workflow = workflow;
    }

    /// <inheritdoc />
    protected override async Task RunAsync(Execution<TContext> execution, CancellationToken cancellationToken = default)
    {
        try { await workflow.RunAsync(execution.Context, cancellationToken).ConfigureAwait(false); } // here we shouldn't remove async keyword 'cause it'll raise an exception outside this catch
        catch (Exception ex) when (True(() => logger.LogError(ex, "Somewhere along the way an exception was thrown and it was eaten. See inner exception"))) { }
    }
}
