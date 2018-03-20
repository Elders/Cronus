using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Cluster.Config;
using Elders.Cronus.EventStore;
using Elders.Cronus.IocContainer;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Middleware;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Snapshotting;
using Elders.Cronus.Projections.Versioning;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline.Config
{
    public class ProjectionMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    {
        public ProjectionMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
        }

        List<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

        private IEnumerable<Type> GetAllEventsFromAssemliesBecauseSomeEventMightNotBePartOfProjections(IEnumerable<Type> projectionHandlers)
        {
            var ievent = typeof(IEvent);
            var ieventHandler = (typeof(IEventHandler<>));
            var allEventTypes = projectionHandlers.SelectMany(y => y.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler))
                .Select(x => x.GetGenericArguments().FirstOrDefault().Assembly).SelectMany(x => x.GetTypes().Where(y => ievent.IsAssignableFrom(y)));
            return allEventTypes;
        }
        public override void Build()
        {
            var hasher = new ProjectionHasher();
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);

                //  May be we want to have separate serializer + transport per type?!?!?
                var transport = builder.Container.Resolve<ITransport>(builder.Name);
                var serializer = builder.Container.Resolve<ISerializer>(null);
                var publisher = transport.GetPublisher<ICommand>(serializer);

                var projectionsMiddleware = new ProjectionsMiddleware(handlerFactory);
                var middleware = processorSettings.HandleMiddleware(projectionsMiddleware);
                var subscriptionMiddleware = new SubscriptionMiddleware();
                var iProjection = typeof(IProjection);

                foreach (var handler in processorSettings.HandlerRegistrations)
                {
                    if (iProjection.IsAssignableFrom(handler))
                    {
                        subscriptionMiddleware.Subscribe(new HandlerSubscriber<IProjection, IEventHandler<IEvent>>(handler, middleware));
                        var projManagerId = new ProjectionVersionManagerId(handler.GetContractId());
                        var command = new RegisterProjection(projManagerId, hasher.CalculateHash(handler).ToString());
                        publisher.Publish(command);
                    }
                }
                var indexSubscriber = builder.Container.Resolve<EventTypeIndexForProjections>();
                subscriptionMiddleware.Subscribe(indexSubscriber);
                return subscriptionMiddleware;
            };

            builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public class ProjectionIndexSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    {
        public ProjectionIndexSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
        }

        List<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

        private IEnumerable<Type> GetAllEventsFromAssemliesBecauseSomeEventMightNotBePartOfProjections(IEnumerable<Type> projectionHandlers)
        {
            var ievent = typeof(IEvent);
            var ieventHandler = (typeof(IEventHandler<>));
            var allEventTypes = projectionHandlers.SelectMany(y => y.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == ieventHandler))
                .Select(x => x.GetGenericArguments().FirstOrDefault().Assembly).SelectMany(x => x.GetTypes().Where(y => ievent.IsAssignableFrom(y)));
            return allEventTypes;
        }
        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings;

            Func<EventTypeIndexForProjections> eventIndexForProjectionsFactory = () =>
            {
                //  May be we want to have separate serializer + transport per type?!?!?
                var transport = builder.Container.Resolve<ITransport>(builder.Name);
                var serializer = builder.Container.Resolve<ISerializer>(null);
                var publisher = transport.GetPublisher<ICommand>(serializer);
                var allEventTypes = GetAllEventsFromAssemliesBecauseSomeEventMightNotBePartOfProjections(processorSettings.HandlerRegistrations);
                var projectionStore = builder.Container.Resolve<IProjectionStore>(builder.Name);

                var indexSubscriber = new EventTypeIndexForProjections(allEventTypes, publisher, projectionStore, new StupidProjectionStore(projectionStore));
                return indexSubscriber;
            };

            builder.Container.RegisterSingleton<EventTypeIndexForProjections>(() => eventIndexForProjectionsFactory());
        }
    }

    public class PortMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    {
        public PortMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
        }

        List<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);
                var publisher = builder.Container.Resolve<IPublisher<ICommand>>(builder.Name);
                var portsMiddleware = new PortsMiddleware(handlerFactory, publisher);
                var middleware = processorSettings.HandleMiddleware(portsMiddleware);
                var subscriptionMiddleware = new SubscriptionMiddleware();
                foreach (var reg in (this as ISubscrptionMiddlewareSettings).HandlerRegistrations)
                {
                    if (typeof(IPort).IsAssignableFrom(reg))
                        subscriptionMiddleware.Subscribe(new HandlerSubscriber<IPort, IEventHandler<IEvent>>(reg, middleware));
                }
                return subscriptionMiddleware;
            };
            builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public class SagaMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    {
        public SagaMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
        }

        List<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);
                var commandPublisher = builder.Container.Resolve<IPublisher<ICommand>>(builder.Name);
                var schedulePublisher = builder.Container.Resolve<IPublisher<IScheduledMessage>>(builder.Name);
                var sagasMiddleware = new SagasMiddleware(handlerFactory, commandPublisher, schedulePublisher);
                var middleware = processorSettings.HandleMiddleware(sagasMiddleware);
                var subscriptionMiddleware = new SubscriptionMiddleware();
                foreach (var reg in (this as ISubscrptionMiddlewareSettings).HandlerRegistrations)
                {
                    if (typeof(ISaga).IsAssignableFrom(reg))
                        subscriptionMiddleware.Subscribe(new SagaSubscriber(reg, middleware));
                }
                return subscriptionMiddleware;
            };
            builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public class ApplicationServiceMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    {
        public ApplicationServiceMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
        }

        List<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);
                var repository = builder.Container.Resolve<IAggregateRepository>(builder.Name);
                var publisher = builder.Container.Resolve<IPublisher<IEvent>>(builder.Name);

                //create extension method UseApplicationMiddleware instead of instance here.
                var applicationServiceMiddleware = new ApplicationServiceMiddleware(handlerFactory, repository, publisher);
                var middleware = processorSettings.HandleMiddleware(applicationServiceMiddleware);
                var subscriptionMiddleware = new SubscriptionMiddleware();
                foreach (var reg in (this as ISubscrptionMiddlewareSettings).HandlerRegistrations)
                {
                    if (typeof(IAggregateRootApplicationService).IsAssignableFrom(reg))
                        subscriptionMiddleware.Subscribe(new HandlerSubscriber<IAggregateRootApplicationService, ICommandHandler<ICommand>>(reg, middleware));
                }
                return subscriptionMiddleware;
            };
            builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public class SystemServiceMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    {
        public SystemServiceMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
        }

        List<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);

                var publisher = builder.Container.Resolve<IPublisher<IEvent>>(builder.Name);
                var repository = builder.Container.Resolve<IAggregateRepository>(builder.Name);
                var systemServiceMiddleware = new ApplicationServiceMiddleware(handlerFactory, repository, publisher);
                var middleware = processorSettings.HandleMiddleware(systemServiceMiddleware);
                var subscriptionMiddleware = new SubscriptionMiddleware();
                foreach (var reg in (this as ISubscrptionMiddlewareSettings).HandlerRegistrations)
                {
                    if (typeof(ISystemService).IsAssignableFrom(reg) && reg.IsClass)
                        subscriptionMiddleware.Subscribe(new SystemServiceSubscriber(reg, middleware));
                }
                return subscriptionMiddleware;
            };
            builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public class SystemProjectionMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    {
        public SystemProjectionMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
        }

        List<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

        public override void Build()
        {
            var hasher = new ProjectionHasher();
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);

                var clusterServiceMiddleware = new SystemMiddleware(handlerFactory);
                var middleware = processorSettings.HandleMiddleware(clusterServiceMiddleware);
                var subscriptionMiddleware = new SubscriptionMiddleware();
                var clusterSettings = builder.Container.Resolve<IClusterSettings>();
                string nodeId = $"{clusterSettings.CurrentNodeName}@{clusterSettings.ClusterName}";
                foreach (var reg in (this as ISubscrptionMiddlewareSettings).HandlerRegistrations)
                {
                    string subscriberId = reg.FullName + "." + nodeId;
                    if (typeof(ISystemProjection).IsAssignableFrom(reg) && reg.IsClass)
                    {
                        subscriptionMiddleware.Subscribe(new SystemProjectionSubscriber(reg, middleware, subscriberId));

                        var transport = builder.Container.Resolve<ITransport>(builder.Name);
                        var serializer = builder.Container.Resolve<ISerializer>(null);
                        var publisher = transport.GetPublisher<ICommand>(serializer);
                        var projManagerId = new ProjectionVersionManagerId(reg.GetContractId());
                        var command = new RegisterProjection(projManagerId, hasher.CalculateHash(reg).ToString());
                        publisher.Publish(command);
                    }
                }
                return subscriptionMiddleware;
            };
            builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public class SystemSagaMessageProcessorSettings : SagaMessageProcessorSettings
    {
        public SystemSagaMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {

        }

        public override void Build()
        {
            base.Build();
            var builder = this as ISettingsBuilder;


            Func<IProjectionRepository> projectionRepository = () => builder.Container.Resolve<IProjectionRepository>(builder.Name);
            Func<IProjectionStore> projectionStore = () => builder.Container.Resolve<IProjectionStore>(builder.Name);
            Func<ISnapshotStore> snapshotStore = () => builder.Container.Resolve<ISnapshotStore>(builder.Name);
            Func<EventTypeIndexForProjections> eventIndexForProjections = () => builder.Container.Resolve<EventTypeIndexForProjections>();
            Func<IEventStoreFactory> eventStoreFactory = () => builder.Container.Resolve<IEventStoreFactory>(builder.Name);
            ITenantResolver tenantResolver = new DefaultTenantResolver();

            builder.Container.RegisterSingleton<ProjectionPlayer>(() => new ProjectionPlayer(eventStoreFactory(), projectionStore(), projectionRepository(), snapshotStore(), eventIndexForProjections(), tenantResolver), builder.Name);
            builder.Container.RegisterSingleton<EventStoreIndexPlayer>(() => new EventStoreIndexPlayer(eventStoreFactory(), projectionStore(), projectionRepository()), builder.Name);
        }
    }

    public static class MessageProcessorSettingsExtensions
    {
        public static T UseProjectionsIndex<T>(this T self, Action<ProjectionIndexSettings> configure) where T : ProjectionConsumerSettings
        {
            ProjectionIndexSettings settings = new ProjectionIndexSettings(self);
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseProjections<T>(this T self, Action<ProjectionMessageProcessorSettings> configure) where T : ProjectionConsumerSettings
        {
            ProjectionMessageProcessorSettings settings = new ProjectionMessageProcessorSettings(self);
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UsePorts<T>(this T self, Action<PortMessageProcessorSettings> configure) where T : PortConsumerSettings
        {
            PortMessageProcessorSettings settings = new PortMessageProcessorSettings(self);
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseSagas<T>(this T self, Action<SagaMessageProcessorSettings> configure) where T : SagaConsumerSettings
        {
            SagaMessageProcessorSettings settings = new SagaMessageProcessorSettings(self);
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseApplicationServices<T>(this T self, Action<ApplicationServiceMessageProcessorSettings> configure) where T : IConsumerSettings<ICommand>
        {
            ApplicationServiceMessageProcessorSettings settings = new ApplicationServiceMessageProcessorSettings(self);
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseSystemServices<T>(this T self, Action<SystemServiceMessageProcessorSettings> configure) where T : IConsumerSettings<ICommand>
        {
            SystemServiceMessageProcessorSettings settings = new SystemServiceMessageProcessorSettings(self);
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseSystemProjections<T>(this T self, Action<SystemProjectionMessageProcessorSettings> configure) where T : IConsumerSettings<IEvent>
        {
            SystemProjectionMessageProcessorSettings settings = new SystemProjectionMessageProcessorSettings(self);
            if (configure != null)
                configure(settings);
            ;
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseSystemSagas<T>(this T self, Action<SystemSagaMessageProcessorSettings> configure) where T : SagaConsumerSettings
        {
            SystemSagaMessageProcessorSettings settings = new SystemSagaMessageProcessorSettings(self);
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }
    }
}
