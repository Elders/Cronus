using System;
using System.Collections.Generic;
using Elders.Cronus.IocContainer;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Middleware;

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

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);

                var projectionsMiddleware = new ProjectionsMiddleware(handlerFactory);
                var middleware = processorSettings.HandleMiddleware(projectionsMiddleware);
                var subscriptionMiddleware = new SubscriptionMiddleware();
                foreach (var reg in processorSettings.HandlerRegistrations)
                {
                    if (typeof(IProjection).IsAssignableFrom(reg))
                        subscriptionMiddleware.Subscribe(new HandleSubscriber<IProjection, IEventHandler<IEvent>>(reg, middleware));
                }
                return subscriptionMiddleware;
            };
            builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
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
                        subscriptionMiddleware.Subscribe(new HandleSubscriber<IPort, IEventHandler<IEvent>>(reg, middleware));
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
                        subscriptionMiddleware.Subscribe(new HandleSubscriber<IAggregateRootApplicationService, ICommandHandler<ICommand>>(reg, middleware));
                }
                return subscriptionMiddleware;
            };
            builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public static class MessageProcessorSettingsExtensions
    {
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
    }
}
