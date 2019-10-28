using Elders.Cronus.FaultHandling;
using Elders.Cronus.Projections;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Elders.Cronus.MessageProcessing
{
    public class ProjectionSubscriberWorkflow : ISubscriberWorkflow<IProjection>
    {
        private readonly IServiceProvider serviceProvider;

        public ProjectionSubscriberWorkflow(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IWorkflow GetWorkflow()
        {
            var messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            var scopedWorkflow = new ScopedMessageWorkflow(serviceProvider, messageHandleWorkflow);
            messageHandleWorkflow.Finalize.Use(new ProjectionsWorkflow(x => ScopedMessageWorkflow.GetScope(x).ServiceProvider.GetRequiredService<IProjectionWriter>()));
            var projectionsWorkflow = new InMemoryRetryWorkflow<HandleContext>(scopedWorkflow);

            return projectionsWorkflow;
        }
    }
}
