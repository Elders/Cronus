using Elders.Cronus.Workflow;
using System;

namespace Elders.Cronus.MessageProcessing
{
    public class TriggersSubscriberWorkflow<TTrigger> : ISubscriberWorkflowFactory<TTrigger>
    {
        private readonly IServiceProvider serviceProvider;

        public TriggersSubscriberWorkflow(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IWorkflow GetWorkflow()
        {
            MessageHandleWorkflow messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            ScopedMessageWorkflow scopedWorkflow = new ScopedMessageWorkflow(serviceProvider, messageHandleWorkflow);

            return scopedWorkflow;
        }
    }
}
