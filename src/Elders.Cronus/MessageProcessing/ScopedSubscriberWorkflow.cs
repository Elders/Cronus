using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace Elders.Cronus.MessageProcessing
{
    public class ScopedSubscriberWorkflow<T> : ISubscriberWorkflowFactory<T>
    {
        private readonly IServiceProvider serviceProvider;

        public ScopedSubscriberWorkflow(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IWorkflow GetWorkflow()
        {
            MessageHandleWorkflow messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            ScopedMessageWorkflow scopedWorkflow = new ScopedMessageWorkflow(serviceProvider, messageHandleWorkflow);
            DiagnosticsWorkflow<HandleContext> diagnosticsWorkflow = new DiagnosticsWorkflow<HandleContext>(scopedWorkflow, serviceProvider.GetRequiredService<DiagnosticListener>());

            return diagnosticsWorkflow;
        }
    }
}
