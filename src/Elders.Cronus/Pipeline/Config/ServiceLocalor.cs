using System;
//using Elders.Cronus.Cluster.Config;

namespace Elders.Cronus.Pipeline.Config
{
    public class ServiceLocalor
    {
        private readonly Func<Type, object> factory;

        public ServiceLocalor(Func<Type, object> factory)
        {
            this.factory = factory;
        }

        public T Create<T>() => (T)factory(typeof(T));
    }

    //public class SystemServiceMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    //{
    //    public SystemServiceMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
    //    {
    //        (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
    //    }

    //    IEnumerable<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

    //    Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

    //    Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

    //    public override void Build()
    //    {
    //        var builder = this as ISettingsBuilder;
    //        var processorSettings = this as ISubscrptionMiddlewareSettings;
    //        Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
    //        {
    //            var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);

    //            var publisher = builder.Container.Resolve<IPublisher<IEvent>>(builder.Name);
    //            var repository = builder.Container.Resolve<IAggregateRepository>(builder.Name);
    //            var systemServiceMiddleware = new ApplicationServiceMiddleware(handlerFactory, repository, publisher);
    //            var middleware = processorSettings.HandleMiddleware(systemServiceMiddleware);
    //            var subscriptionMiddleware = new SubscriptionMiddleware();
    //            foreach (var reg in (this as ISubscrptionMiddlewareSettings).HandlerRegistrations)
    //            {
    //                if (typeof(ISystemService).IsAssignableFrom(reg) && reg.IsClass)
    //                    subscriptionMiddleware.Subscribe(new HandlerSubscriber(reg, middleware));
    //            }
    //            return subscriptionMiddleware;
    //        };
    //        builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
    //    }
    //}

    //public class SystemProjectionMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    //{
    //    public SystemProjectionMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
    //    {
    //        (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
    //    }

    //    IEnumerable<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

    //    Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

    //    Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

    //    public override void Build()
    //    {
    //        var hasher = new ProjectionHasher();
    //        var builder = this as ISettingsBuilder;
    //        var processorSettings = this as ISubscrptionMiddlewareSettings;
    //        Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
    //        {
    //            var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);

    //            var clusterServiceMiddleware = new SystemMiddleware(handlerFactory);
    //            var middleware = processorSettings.HandleMiddleware(clusterServiceMiddleware);
    //            var subscriptionMiddleware = new SubscriptionMiddleware();
    //            var clusterSettings = builder.Container.Resolve<IClusterSettings>();
    //            string nodeId = $"{clusterSettings.CurrentNodeName}@{clusterSettings.ClusterName}";
    //            foreach (var reg in (this as ISubscrptionMiddlewareSettings).HandlerRegistrations)
    //            {
    //                string subscriberId = reg.FullName + "." + nodeId;
    //                if (typeof(ISystemProjection).IsAssignableFrom(reg) && reg.IsClass)
    //                {
    //                    subscriptionMiddleware.Subscribe(new HandlerSubscriber(reg, middleware, subscriberId));

    //                    var transport = builder.Container.Resolve<ITransport>(builder.Name);
    //                    var serializer = builder.Container.Resolve<ISerializer>(null);
    //                    var publisher = transport.GetPublisher<ICommand>(serializer);
    //                    var projManagerId = new ProjectionVersionManagerId(reg.GetContractId());
    //                    var command = new RegisterProjection(projManagerId, hasher.CalculateHash(reg).ToString());
    //                    publisher.Publish(command);
    //                }
    //            }
    //            return subscriptionMiddleware;
    //        };
    //        builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
    //    }
    //}

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

    //public static class MessageProcessorSettingsExtensions
    //{
    //    public static T UseProjectionsIndex<T>(this T self, Action<ProjectionIndexSettings> configure) where T : ProjectionConsumerSettings
    //    {
    //        ProjectionIndexSettings settings = new ProjectionIndexSettings(self);
    //        if (configure != null)
    //            configure(settings);

    //        (settings as ISettingsBuilder).Build();
    //        return self;
    //    }

    //    public static T UseProjections<T>(this T self, Action<ProjectionMessageProcessorSettings> configure) where T : ProjectionConsumerSettings
    //    {
    //        ProjectionMessageProcessorSettings settings = new ProjectionMessageProcessorSettings(self);
    //        if (configure != null)
    //            configure(settings);

    //        (settings as ISettingsBuilder).Build();
    //        return self;
    //    }

    //    public static T UsePorts<T>(this T self, Action<PortMessageProcessorSettings> configure) where T : PortConsumerSettings
    //    {
    //        PortMessageProcessorSettings settings = new PortMessageProcessorSettings(self);
    //        if (configure != null)
    //            configure(settings);

    //        (settings as ISettingsBuilder).Build();
    //        return self;
    //    }

    //    public static T UseSagas<T>(this T self, Action<SagaMessageProcessorSettings> configure) where T : SagaConsumerSettings
    //    {
    //        SagaMessageProcessorSettings settings = new SagaMessageProcessorSettings(self);
    //        if (configure != null)
    //            configure(settings);

    //        (settings as ISettingsBuilder).Build();
    //        return self;
    //    }

    //    public static T UseApplicationServices<T>(this T self, Action<ApplicationServiceMessageProcessorSettings> configure) where T : IConsumerSettings<ICommand>
    //    {
    //        ApplicationServiceMessageProcessorSettings settings = new ApplicationServiceMessageProcessorSettings(self);
    //        if (configure != null)
    //            configure(settings);

    //        (settings as ISettingsBuilder).Build();
    //        return self;
    //    }

    //    public static T UseSystemServices<T>(this T self, Action<SystemServiceMessageProcessorSettings> configure) where T : IConsumerSettings<ICommand>
    //    {
    //        SystemServiceMessageProcessorSettings settings = new SystemServiceMessageProcessorSettings(self);
    //        if (configure != null)
    //            configure(settings);

    //        (settings as ISettingsBuilder).Build();
    //        return self;
    //    }

    //    public static T UseSystemProjections<T>(this T self, Action<SystemProjectionMessageProcessorSettings> configure) where T : IConsumerSettings<IEvent>
    //    {
    //        SystemProjectionMessageProcessorSettings settings = new SystemProjectionMessageProcessorSettings(self);
    //        if (configure != null)
    //            configure(settings);

    //        (settings as ISettingsBuilder).Build();
    //        return self;
    //    }

    //    public static T UseSystemSagas<T>(this T self, Action<SystemSagaMessageProcessorSettings> configure) where T : SagaConsumerSettings
    //    {
    //        SystemSagaMessageProcessorSettings settings = new SystemSagaMessageProcessorSettings(self);
    //        if (configure != null)
    //            configure(settings);

    //        (settings as ISettingsBuilder).Build();
    //        return self;
    //    }
    //}
}
