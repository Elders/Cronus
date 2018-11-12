using System;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;

namespace Elders.Cronus
{
    public class SagasStartup : StartupBase<ISaga>
    {
        private readonly ITenantList tenants;
        private readonly ITenantResolver tenantResolver;
        private readonly IServiceProvider ioc;
        private readonly SubscriberCollection<ISaga> subscriberCollection;
        private readonly TypeContainer<ISaga> typeContainer;

        private ScopedMessageWorkflow defaultWorkflow;

        public SagasStartup(IServiceProvider ioc, IConsumer<ISaga> consumer, SubscriberCollection<ISaga> subscriberCollection, TypeContainer<ISaga> typeContainer, ITenantList tenants, ITenantResolver tenantResolver)
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
