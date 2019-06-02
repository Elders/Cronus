using Elders.Cronus.FaultHandling;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Workflow;
using System;

namespace Elders.Cronus.MessageProcessing
{
    public class ApplicationServiceSubscriberWorkflow : ISubscriberWorkflow<IApplicationService>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ITenantResolver tenantResolver;

        public ApplicationServiceSubscriberWorkflow(IServiceProvider serviceProvider, ITenantResolver tenantResolver)
        {
            this.serviceProvider = serviceProvider;
            this.tenantResolver = tenantResolver;
        }

        public IWorkflow GetWorkflow()
        {
            var messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            var scopedWorkflow = new ScopedMessageWorkflow(serviceProvider, messageHandleWorkflow, tenantResolver);
            var customWorkflow = new InMemoryRetryWorkflow<HandleContext>(scopedWorkflow);

            return customWorkflow;
        }
    }
}
