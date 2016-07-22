using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.MessageProcessingMiddleware;
using Elders.Cronus.Middleware;
using Elders.Cronus.Netflix;

namespace Elders.Cronus.Pipeline.Config
{
    public class ProjectionMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings<IEvent>
    {
        public ProjectionMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
        {
            (this as ISubscrptionMiddlewareSettings<IEvent>).ActualHandle = new DynamicMessageHandle();
        }

        List<Type> ISubscrptionMiddlewareSettings<IEvent>.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings<IEvent>.HandlerFactory { get; set; }

        Middleware<MessageHandlerMiddleware.HandleContext> ISubscrptionMiddlewareSettings<IEvent>.ActualHandle { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings<IEvent>;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);

                var projectionsMiddleware = new ProjectionsMiddleware(handlerFactory);
                var actualHandleHook = (this as ISubscrptionMiddlewareSettings<IEvent>).ActualHandle;
                projectionsMiddleware.ActualHandle = actualHandleHook;
                var subscriptionMiddleware = new Netflix.SubscriptionMiddleware();
                foreach (var reg in processorSettings.HandlerRegistrations)
                {
                    subscriptionMiddleware.Subscribe(new Netflix.ProjectionSubscriber(reg, projectionsMiddleware));
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
            (this as ISubscrptionMiddlewareSettings<IEvent>).ActualHandle = new DynamicMessageHandle();
        }

        List<Type> ISubscrptionMiddlewareSettings<IEvent>.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings<IEvent>.HandlerFactory { get; set; }

        Middleware<MessageHandlerMiddleware.HandleContext> ISubscrptionMiddlewareSettings<IEvent>.ActualHandle { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as ISubscrptionMiddlewareSettings<IEvent>;
            Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);
                var publisher = builder.Container.Resolve<IPublisher<ICommand>>(builder.Name);

                var portsMiddleware = new PortsMiddleware(handlerFactory, publisher);
                var actualHandleHook = (this as ISubscrptionMiddlewareSettings<IEvent>).ActualHandle;
                portsMiddleware.ActualHandle = actualHandleHook;
                var subscriptionMiddleware = new Netflix.SubscriptionMiddleware();
                foreach (var reg in (this as ISubscrptionMiddlewareSettings<IEvent>).HandlerRegistrations)
                {
                    subscriptionMiddleware.Subscribe(new Netflix.PortSubscriber(reg, portsMiddleware));
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
            (this as ISubscrptionMiddlewareSettings<ICommand>).ActualHandle = new DynamicMessageHandle();
        }

        List<Type> ISubscrptionMiddlewareSettings<ICommand>.HandlerRegistrations { get; set; }

        Func<Type, object> ISubscrptionMiddlewareSettings<ICommand>.HandlerFactory { get; set; }

        Middleware<MessageHandlerMiddleware.HandleContext> ISubscrptionMiddlewareSettings<ICommand>.ActualHandle { get; set; }

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
                applicationServiceMiddleware.ActualHandle = (this as ISubscrptionMiddlewareSettings<ICommand>).ActualHandle;


                var subscriptionMiddleware = new Netflix.SubscriptionMiddleware();
                foreach (var reg in (this as ISubscrptionMiddlewareSettings<ICommand>).HandlerRegistrations)
                {
                    subscriptionMiddleware.Subscribe(new Netflix.ApplicationServiceSubscriber(reg, applicationServiceMiddleware));
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
