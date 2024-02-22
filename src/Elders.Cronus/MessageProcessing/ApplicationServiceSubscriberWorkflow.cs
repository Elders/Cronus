using Elders.Cronus.FaultHandling;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Elders.Cronus.MessageProcessing;

public class ApplicationServiceSubscriberWorkflow : ISubscriberWorkflowFactory<IApplicationService>
{
    private readonly IServiceProvider serviceProvider;

    public ApplicationServiceSubscriberWorkflow(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IWorkflow GetWorkflow()
    {
        ILogger<InMemoryRetryWorkflow<HandleContext>> logger = serviceProvider.GetRequiredService<ILogger<InMemoryRetryWorkflow<HandleContext>>>();

        MessageHandleWorkflow messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
        ScopedMessageWorkflow scopedWorkflow = new ScopedMessageWorkflow(messageHandleWorkflow, serviceProvider);
        InMemoryRetryWorkflow<HandleContext> retryableWorkflow = new InMemoryRetryWorkflow<HandleContext>(scopedWorkflow, logger);
        DiagnosticsWorkflow<HandleContext> diagnosticsWorkflow = new DiagnosticsWorkflow<HandleContext>(retryableWorkflow, serviceProvider.GetRequiredService<DiagnosticListener>(), serviceProvider.GetRequiredService<ActivitySource>());
        ExceptionEaterWorkflow<HandleContext> exceptionEater = new ExceptionEaterWorkflow<HandleContext>(diagnosticsWorkflow);

        return exceptionEater;
    }
}
