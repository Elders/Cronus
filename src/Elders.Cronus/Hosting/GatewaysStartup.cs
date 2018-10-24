using System;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;

namespace Elders.Cronus
{
    public class GatewaysStartup : StartupBase<IGateway>
    {
        private readonly ITenantList tenants;
        private readonly SubscriberCollection<IGateway> subscriberCollection;
        private readonly TypeContainer<IGateway> typeContainer;
        private readonly ScopedMessageWorkflow defaultWorkflow;

        public GatewaysStartup(IServiceProvider ioc, IConsumer<IGateway> consumer, SubscriberCollection<IGateway> subscriberCollection, TypeContainer<IGateway> typeContainer, ITenantList tenants)
            : base(consumer)
        {
            this.subscriberCollection = subscriberCollection;
            this.typeContainer = typeContainer;
            this.tenants = tenants;

            defaultWorkflow = new ScopedMessageWorkflow(ioc, new MessageHandleWorkflow(new CreateScopedHandlerWorkflow()));
            RegisterSubscribers();
        }

        void RegisterSubscribers()
        {
            foreach (var type in typeContainer.Items)
            {
                var handlerSubscriber = new HandlerSubscriber(type, defaultWorkflow);
                subscriberCollection.Subscribe(handlerSubscriber);
            }
        }
    }
}
