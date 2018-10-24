using System;
using Elders.Cronus.FaultHandling;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;
using Elders.Cronus.Multitenancy;

namespace Elders.Cronus
{
    public class ApplicationServicesStartup : StartupBase<IAggregateRootApplicationService>
    {
        private readonly ITenantList tenants;
        private readonly SubscriberCollection<IAggregateRootApplicationService> subscriberCollection;
        private readonly TypeContainer<IAggregateRootApplicationService> typeContainer;
        private Workflow<HandleContext> customWorkflow;

        public ApplicationServicesStartup(IServiceProvider ioc, IConsumer<IAggregateRootApplicationService> consumer, SubscriberCollection<IAggregateRootApplicationService> subscriberCollection, TypeContainer<IAggregateRootApplicationService> typeContainer, ITenantList tenants)
            : base(consumer)
        {
            this.subscriberCollection = subscriberCollection;
            this.typeContainer = typeContainer;
            this.tenants = tenants;

            var messageHandleWorkflow = new MessageHandleWorkflow(new CreateScopedHandlerWorkflow());
            var scopedWorkflow = new ScopedMessageWorkflow(ioc, messageHandleWorkflow);
            customWorkflow = new InMemoryRetryWorkflow<HandleContext>(scopedWorkflow);
            RegisterSubscribers();
        }


        void RegisterSubscribers()
        {
            foreach (var type in typeContainer.Items)
            {
                var handlerSubscriber = new HandlerSubscriber(type, customWorkflow);
                subscriberCollection.Subscribe(handlerSubscriber);
            }
        }
    }
}
