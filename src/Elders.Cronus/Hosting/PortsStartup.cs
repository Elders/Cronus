using System;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;

namespace Elders.Cronus
{
    public class PortsStartup : StartupBase<IPort>
    {
        private readonly ITenantList tenants;
        private readonly ITenantResolver tenantResolver;
        private readonly IServiceProvider ioc;
        private readonly SubscriberCollection<IPort> subscriberCollection;
        private readonly TypeContainer<IPort> typeContainer;
        private ScopedMessageWorkflow defaultWorkflow;

        public PortsStartup(IServiceProvider ioc, IConsumer<IPort> consumer, SubscriberCollection<IPort> subscriberCollection, TypeContainer<IPort> typeContainer, ITenantList tenants, ITenantResolver tenantResolver)
            : base(consumer)
        {
            this.ioc = ioc;
            this.subscriberCollection = subscriberCollection;
            this.typeContainer = typeContainer;
            this.tenants = tenants;
            this.tenantResolver = tenantResolver;

        }

        public override void Start()
        {
            defaultWorkflow = new ScopedMessageWorkflow(ioc, new MessageHandleWorkflow(new CreateScopedHandlerWorkflow()), tenantResolver);
            RegisterSubscribers();

            base.Start();
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
