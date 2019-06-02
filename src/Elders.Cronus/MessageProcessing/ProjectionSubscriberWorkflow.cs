using Elders.Cronus.FaultHandling;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Elders.Cronus.MessageProcessing
{
    public class ProjectionSubscriberWorkflow : ISubscriberWorkflow<IProjection>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ITenantResolver tenantResolver;

        public ProjectionSubscriberWorkflow(IServiceProvider serviceProvider, ITenantResolver tenantResolver)
        {
            this.serviceProvider = serviceProvider;
            this.tenantResolver = tenantResolver;
        }

        public IWorkflow GetWorkflow()
        {
            var messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            var scopedWorkflow = new ScopedMessageWorkflow(serviceProvider, messageHandleWorkflow, tenantResolver);
            messageHandleWorkflow.Finalize.Use(new ProjectionsWorkflow(x => ScopedMessageWorkflow.GetScope(x).ServiceProvider.GetRequiredService<IProjectionWriter>()));
            var projectionsWorkflow = new InMemoryRetryWorkflow<HandleContext>(scopedWorkflow);

            return projectionsWorkflow;
        }
    }
}
