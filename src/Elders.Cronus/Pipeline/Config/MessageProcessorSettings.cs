using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.FaultHandling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.MessageProcessingMiddleware;
using Elders.Cronus.Middleware;
using Elders.Cronus.Netflix;

namespace Elders.Cronus.Pipeline.Config
{
    public static class MiddlewareExtensions
    {
        public static ISubscrptionMiddlewareSettings<T> Middleware<T>(this ISubscrptionMiddlewareSettings<T> self, Func<Middleware<HandleContext>, Middleware<HandleContext>> middlewareConfig)
            where T : IMessage
        {
            self.HandleMiddleware = middlewareConfig;
            return self;
        }
    }

    public static class RetryExtensions
    {
        public static Middleware<TContext> UseRetries<TContext>(this Middleware<TContext> self)
        {
            return new InMemoryRetryMiddleware<TContext>(self);
        }
    }

    public class ProjectionMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings<IEvent>
    {
        public ProjectionMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings<IEvent>).HandleMiddleware = (x) => x;
        }

        List<Type> ISubscrptionMiddlewareSettings<IEvent>.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings<IEvent>.HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings<IEvent>.HandleMiddleware { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings<IEvent>;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);

                var projectionsMiddleware = new ProjectionsMiddleware(handlerFactory);
                var middleware = processorSettings.HandleMiddleware(projectionsMiddleware);
                var subscriptionMiddleware = new Netflix.SubscriptionMiddleware();
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

    public class PortMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings<IEvent>
    {
        public PortMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings<IEvent>).HandleMiddleware = (x) => x;
        }

        List<Type> ISubscrptionMiddlewareSettings<IEvent>.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings<IEvent>.HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings<IEvent>.HandleMiddleware { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings<IEvent>;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);
                var publisher = builder.Container.Resolve<IPublisher<ICommand>>(builder.Name);
                var portsMiddleware = new PortsMiddleware(handlerFactory, publisher);
                var middleware = processorSettings.HandleMiddleware(portsMiddleware);
                var subscriptionMiddleware = new Netflix.SubscriptionMiddleware();
                foreach (var reg in (this as ISubscrptionMiddlewareSettings<IEvent>).HandlerRegistrations)
                {
                    if (typeof(IPort).IsAssignableFrom(reg))
                        subscriptionMiddleware.Subscribe(new HandleSubscriber<IPort, IEventHandler<IEvent>>(reg, middleware));
                }
                return subscriptionMiddleware;
            };
            builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public class ApplicationServiceMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings<ICommand>
    {
        public ApplicationServiceMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings<ICommand>).HandleMiddleware = (x) => x;
        }

        List<Type> ISubscrptionMiddlewareSettings<ICommand>.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings<ICommand>.HandlerFactory { get; set; }

        Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings<ICommand>.HandleMiddleware { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings<ICommand>;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);
                var repository = builder.Container.Resolve<IAggregateRepository>(builder.Name);
                var publisher = builder.Container.Resolve<IPublisher<IEvent>>(builder.Name);

                //create extension methis UseApplicationMiddleware instead of instance here.
                var applicationServiceMiddleware = new ApplicationServiceMiddleware(handlerFactory, repository, publisher);
                var middleware = processorSettings.HandleMiddleware(applicationServiceMiddleware);
                var subscriptionMiddleware = new Netflix.SubscriptionMiddleware();
                foreach (var reg in (this as ISubscrptionMiddlewareSettings<ICommand>).HandlerRegistrations)
                {
                    if (typeof(IAggregateRootApplicationService).IsAssignableFrom(reg))
                        subscriptionMiddleware.Subscribe(new HandleSubscriber<IAggregateRootApplicationService, ICommandHandler<ICommand>>(reg, middleware));
                }
                return subscriptionMiddleware;
            };
            builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public static class MessageProcessorWithSafeBatchSettingsExtensions
    {
        public static T UseProjections<T>(this T self, Action<ProjectionMessageProcessorSettings> configure) where T : IConsumerSettings<IEvent>
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

        public static T UseApplicationServices<T>(this T self, Action<ApplicationServiceMessageProcessorSettings> configure) where T : IConsumerSettings<ICommand>
        {
            ApplicationServiceMessageProcessorSettings settings = new ApplicationServiceMessageProcessorSettings(self);
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseApplicationServiceMiddleware<T>(this T self, Action<ApplicationServiceMessageProcessorSettings> configure) where T : IConsumerSettings<ICommand>
        {
            ApplicationServiceMessageProcessorSettings settings = new ApplicationServiceMessageProcessorSettings(self);
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }
    }
}
