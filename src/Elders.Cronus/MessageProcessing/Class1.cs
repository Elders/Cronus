using Elders.Cronus.FaultHandling;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Elders.Cronus.MessageProcessing
{
    public interface ISubscriberFactory<T>
    {
        ISubscriber Create(Type handlerType);
    }
    public class HandlerSubscriberFactory<T> : ISubscriberFactory<T>
    {
        private readonly Workflow<HandleContext> workflow;

        public HandlerSubscriberFactory(ISubscriberWorkflow<T> subscriberWorkflow)
        {
            var asd = subscriberWorkflow.GetWorkflow();
            workflow = asd as Workflow<HandleContext>;

            var gg = (Workflow<HandleContext>)asd;
        }

        public ISubscriber Create(Type handlerType)
        {
            return new HandlerSubscriber(handlerType, workflow);
        }
    }

    public interface ISubscriberWorkflow<T>
    {
        IWorkflow GetWorkflow();
    }
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

    public interface ISubscriberFinder<T>
    {
        IEnumerable<Type> Find();
    }
    public class SubscriberFinder<T> : ISubscriberFinder<T>
    {
        private readonly TypeContainer<T> typeContainer;

        public SubscriberFinder(TypeContainer<T> typeContainer)
        {
            this.typeContainer = typeContainer;
        }

        public IEnumerable<Type> Find()
        {
            return typeContainer.Items;
        }
    }

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
