using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Pipeline.Config
{
    //public class ProjectionIndexSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    //{
    //    public ProjectionIndexSettings(ISettingsBuilder builder) : base(builder)
    //    {
    //        (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
    //    }

    //    IEnumerable<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

    //    Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

    //    Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

    //    private IEnumerable<Type> GetAllEventsFromAssemliesBecauseSomeEventMightNotBePartOfProjections(IEnumerable<Type> projectionHandlers)
    //    {
    //        var ievent = typeof(IEvent);
    //        var ieventHandler = (typeof(IEventHandler<>));
    //        var allEventTypes = projectionHandlers.SelectMany(y => y.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler))
    //            .Select(x => x.GetGenericArguments().FirstOrDefault().Assembly).SelectMany(x => x.GetTypes().Where(y => ievent.IsAssignableFrom(y)));
    //        return allEventTypes;
    //    }
    //    public override void Build()
    //    {
    //        var builder = this as ISettingsBuilder;
    //        var processorSettings = this as ISubscrptionMiddlewareSettings;

    //        Func<EventTypeIndexForProjections> eventIndexForProjectionsFactory = () =>
    //        {
    //            //  May be we want to have separate serializer + transport per type?!?!?
    //            var transport = builder.Container.Resolve<ITransport>(builder.Name);
    //            var serializer = builder.Container.Resolve<ISerializer>(null);
    //            var publisher = transport.GetPublisher<ICommand>(serializer);
    //            var allEventTypes = GetAllEventsFromAssemliesBecauseSomeEventMightNotBePartOfProjections(processorSettings.HandlerRegistrations);
    //            var projectionStore = builder.Container.Resolve<IProjectionStore>(builder.Name);

    //            var indexSubscriber = new EventTypeIndexForProjections(allEventTypes, publisher, projectionStore, new StupidProjectionStore(projectionStore));
    //            return indexSubscriber;
    //        };

    //        if (builder.Container.IsRegistered(typeof(EventTypeIndexForProjections)) == false)
    //            builder.Container.RegisterSingleton<EventTypeIndexForProjections>(() => eventIndexForProjectionsFactory());
    //    }
    //}

    public class HandlerTypeContainer<T>
    {
        public HandlerTypeContainer(IEnumerable<Type> items)
        {
            var expectedHandlerType = typeof(T);
            Items = items.Where(handlerType => expectedHandlerType.IsAssignableFrom(handlerType)).ToList();
        }

        public List<Type> Items { get; set; }
    }


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
}
