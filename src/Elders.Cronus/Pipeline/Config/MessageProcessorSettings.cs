using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline.Config
{

    public class SubscriprionMiddlewareFactory<T> : ISubscriptionMiddleware<T>
    {
        private readonly ISubscriptionMiddleware<T> internalMiddleware;

        private readonly ServiceLocalor messageHandlerMiddlewareFactory;
        private readonly HandlerTypeContainer<T> handlerTypeContainer;

        public SubscriprionMiddlewareFactory(ServiceLocalor serviceLocator, HandlerTypeContainer<T> handlerTypeContainer)
        {
            this.messageHandlerMiddlewareFactory = serviceLocator;
            this.handlerTypeContainer = handlerTypeContainer;

            internalMiddleware = Create();
        }

        public ISubscriptionMiddleware<T> Create()
        {
            var subscrMiddleware = new SubscriptionMiddleware<T>();
            foreach (var type in handlerTypeContainer.Items)
            {
                var handlerSubscriber = new HandlerSubscriber(type, messageHandlerMiddlewareFactory.Create<MessageHandlerMiddleware>());
                subscrMiddleware.Subscribe(handlerSubscriber);
            }

            return subscrMiddleware;
        }

        public void Subscribe(ISubscriber subscriber)
        {
            internalMiddleware.Subscribe(subscriber);
        }

        public IEnumerable<ISubscriber> GetInterestedSubscribers(CronusMessage message)
        {
            return internalMiddleware.GetInterestedSubscribers(message);
        }

        public IEnumerable<ISubscriber> Subscribers { get { return internalMiddleware.Subscribers; } }

        public void UnsubscribeAll()
        {
            internalMiddleware.UnsubscribeAll();
        }
    }


    //public class SystemSagaMessageProcessorSettings : SagaMessageProcessorSettings
    //{
    //    public SystemSagaMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
    //    {

    //    }

    //    public override void Build()
    //    {
    //        base.Build();
    //        var builder = this as ISettingsBuilder;

    //        if (builder.Container.IsRegistered(typeof(InMemoryProjectionVersionStore)) == false)
    //            builder.Container.RegisterSingleton<InMemoryProjectionVersionStore>(() => new InMemoryProjectionVersionStore());
    //        builder.Container.RegisterSingleton<IProjectionRepository>(() => new ProjectionRepository(builder.Container.Resolve<IProjectionStore>(builder.Name), builder.Container.Resolve<ISnapshotStore>(builder.Name), builder.Container.Resolve<ISnapshotStrategy>(builder.Name), builder.Container.Resolve<InMemoryProjectionVersionStore>()), builder.Name);
    //        Func<IProjectionRepository> projectionRepository = () => builder.Container.Resolve<IProjectionRepository>(builder.Name);
    //        Func<IProjectionStore> projectionStore = () => builder.Container.Resolve<IProjectionStore>(builder.Name);
    //        Func<ISnapshotStore> snapshotStore = () => builder.Container.Resolve<ISnapshotStore>(builder.Name);
    //        Func<EventTypeIndexForProjections> eventIndexForProjections = () => builder.Container.Resolve<EventTypeIndexForProjections>();
    //        Func<IEventStoreFactory> eventStoreFactory = () => builder.Container.Resolve<IEventStoreFactory>(builder.Name);
    //        ITenantResolver tenantResolver = new DefaultTenantResolver();

    //        builder.Container.RegisterSingleton<ProjectionPlayer>(() => new ProjectionPlayer(eventStoreFactory(), projectionStore(), projectionRepository(), snapshotStore(), eventIndexForProjections(), tenantResolver), builder.Name);
    //        builder.Container.RegisterSingleton<EventStoreIndexPlayer>(() => new EventStoreIndexPlayer(eventStoreFactory(), projectionStore(), projectionRepository()), builder.Name);
    //    }
    //}
}
