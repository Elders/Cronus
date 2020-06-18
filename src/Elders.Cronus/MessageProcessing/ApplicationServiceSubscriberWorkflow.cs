using Elders.Cronus.FaultHandling;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace Elders.Cronus.MessageProcessing
{
    public class ApplicationServiceSubscriberWorkflow : ISubscriberWorkflowFactory<IApplicationService>
    {
        private readonly IServiceProvider serviceProvider;

        public ApplicationServiceSubscriberWorkflow(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IWorkflow GetWorkflow()
        {
            MessageHandleWorkflow messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            ScopedMessageWorkflow scopedWorkflow = new ScopedMessageWorkflow(serviceProvider, messageHandleWorkflow);
            InMemoryRetryWorkflow<HandleContext> retryableWorkflow = new InMemoryRetryWorkflow<HandleContext>(scopedWorkflow);
            DiagnosticsWorkflow<HandleContext> diagnosticsWorkflow = new DiagnosticsWorkflow<HandleContext>(retryableWorkflow, serviceProvider.GetRequiredService<DiagnosticListener>());

            return diagnosticsWorkflow;
        }
    }
}
