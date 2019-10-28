using Elders.Cronus.FaultHandling;
using Elders.Cronus.Workflow;
using System;

namespace Elders.Cronus.MessageProcessing
{
    public class ApplicationServiceSubscriberWorkflow : ISubscriberWorkflow<IApplicationService>
    {
        private readonly IServiceProvider serviceProvider;

        public ApplicationServiceSubscriberWorkflow(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IWorkflow GetWorkflow()
        {
            var messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            var scopedWorkflow = new ScopedMessageWorkflow(serviceProvider, messageHandleWorkflow);
            var customWorkflow = new InMemoryRetryWorkflow<HandleContext>(scopedWorkflow);

            return customWorkflow;
        }
    }
}
