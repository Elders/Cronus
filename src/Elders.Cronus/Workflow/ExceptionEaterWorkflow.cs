using System;
using System.Threading.Tasks;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Workflow;

public sealed class ExceptionEaterWorkflow<TContext> : Workflow<TContext> where TContext : HandleContext
{
    private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(DiagnosticsWorkflow<>));

    readonly Workflow<TContext> workflow;

    public ExceptionEaterWorkflow(Workflow<TContext> workflow)
    {
        this.workflow = workflow;
    }
    protected override async Task RunAsync(Execution<TContext> execution)
    {
        try { await workflow.RunAsync(execution.Context).ConfigureAwait(false); } // here we shouldn't elide async kwyword 'cause it'll raise an exception outside this catch
        catch (Exception ex) when (logger.ErrorException(ex, () => "Somewhere along the way an exception was thrown and it was eaten. See inner exception")) { }
    }
}
