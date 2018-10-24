using System;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;

namespace Elders.Cronus
{
    public class PortsStartup : StartupBase<IPort>
    {
        private readonly ITenantList tenants;
        private readonly SubscriberCollection<IPort> subscriberCollection;
        private readonly TypeContainer<IPort> typeContainer;
        private readonly ScopedMessageWorkflow defaultWorkflow;

        public PortsStartup(IServiceProvider ioc, IConsumer<IPort> consumer, SubscriberCollection<IPort> subscriberCollection, TypeContainer<IPort> typeContainer, ITenantList tenants)
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
