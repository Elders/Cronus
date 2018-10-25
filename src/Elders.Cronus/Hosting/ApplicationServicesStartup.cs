using System;
using Elders.Cronus.FaultHandling;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;
using Elders.Cronus.Multitenancy;

namespace Elders.Cronus
{
    public class ApplicationServicesStartup : StartupBase<IApplicationService>
    {
        private readonly ITenantList tenants;
        private readonly SubscriberCollection<IApplicationService> subscriberCollection;
        private readonly TypeContainer<IApplicationService> typeContainer;
        private Workflow<HandleContext> customWorkflow;

        public ApplicationServicesStartup(IServiceProvider ioc, IConsumer<IApplicationService> consumer, SubscriberCollection<IApplicationService> subscriberCollection, TypeContainer<IApplicationService> typeContainer, ITenantList tenants)
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
