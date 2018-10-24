using System;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;

namespace Elders.Cronus
{
    public class SagasStartup : StartupBase<ISaga>
    {
        private readonly ITenantList tenants;
        private readonly SubscriberCollection<ISaga> subscriberCollection;
        private readonly TypeContainer<ISaga> typeContainer;

        private readonly ScopedMessageWorkflow defaultWorkflow;

        public SagasStartup(IServiceProvider ioc, IConsumer<ISaga> consumer, SubscriberCollection<ISaga> subscriberCollection, TypeContainer<ISaga> typeContainer, ITenantList tenants)
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
