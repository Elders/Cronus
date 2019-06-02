using Elders.Cronus.Multitenancy;
using Elders.Cronus.Workflow;
using System;

namespace Elders.Cronus.MessageProcessing
{
    public class ScopedSubscriberWorkflow<T> : ISubscriberWorkflow<T>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ITenantResolver tenantResolver;

        public ScopedSubscriberWorkflow(IServiceProvider serviceProvider, ITenantResolver tenantResolver)
        {
            this.serviceProvider = serviceProvider;
            this.tenantResolver = tenantResolver;
        }

        public IWorkflow GetWorkflow()
        {
            var messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            var scopedWorkflow = new ScopedMessageWorkflow(serviceProvider, messageHandleWorkflow, tenantResolver);

            return scopedWorkflow;
        }
    }
}
