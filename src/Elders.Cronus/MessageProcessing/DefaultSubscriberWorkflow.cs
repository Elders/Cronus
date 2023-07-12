using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace Elders.Cronus.MessageProcessing
{
    public class DefaultSubscriberWorkflow<T> : ISubscriberWorkflowFactory<T>
    {
        private readonly IServiceProvider serviceProvider;

        public DefaultSubscriberWorkflow(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IWorkflow GetWorkflow()
        {
            MessageHandleWorkflow messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            ScopedMessageWorkflow scopedWorkflow = new ScopedMessageWorkflow(serviceProvider, messageHandleWorkflow);
            DiagnosticsWorkflow<HandleContext> diagnosticsWorkflow = new DiagnosticsWorkflow<HandleContext>(scopedWorkflow, serviceProvider.GetRequiredService<DiagnosticListener>(), serviceProvider.GetRequiredService<ActivitySource>());
            ExceptionEaterWorkflow<HandleContext> exceptionEater = new ExceptionEaterWorkflow<HandleContext>(diagnosticsWorkflow);

            return exceptionEater;
        }
    }
}
