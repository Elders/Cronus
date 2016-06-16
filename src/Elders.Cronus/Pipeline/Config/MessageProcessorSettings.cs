using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.MessageProcessingMiddleware;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.Pipeline.Config
{
    public class ProjectionMessageProcessorSettings : SettingsBuilder, IMessageProcessorSettings<IEvent>
    {
        private Func<Type, bool> discriminator;

        public ProjectionMessageProcessorSettings(ISettingsBuilder builder, Func<Type, bool> discriminator) : base(builder)
        {
            this.discriminator = discriminator;
        }

        Dictionary<Type, List<Type>> IMessageProcessorSettings<IEvent>.HandlerRegistrations { get; set; }

        Func<Type, object> IMessageProcessorSettings<IEvent>.HandlerFactory { get; set; }

        string IMessageProcessorSettings<IEvent>.MessageProcessorName { get; set; }

        Middleware<MessageHandlerMiddleware.HandleContext> IMessageProcessorSettings<IEvent>.ActualHandle { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as IMessageProcessorSettings<IEvent>;
            Func<IMessageProcessor> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);

                var projectionsMiddleware = new ProjectionsMiddleware(handlerFactory);
                var actualHandleHook = (this as IMessageProcessorSettings<IEvent>).ActualHandle;
                if (ReferenceEquals(null, actualHandleHook))
                    projectionsMiddleware.ActualHandle.Use(actualHandleHook);

                var messageSubscriptionMiddleware = new MessageSubscriptionsMiddleware();
                IMessageProcessor processor = new CronusMessageProcessorMiddleware(processorSettings.MessageProcessorName, messageSubscriptionMiddleware);

                foreach (var reg in processorSettings.HandlerRegistrations)
                {
                    foreach (var handlerType in reg.Value)
                    {
                        if (discriminator == null || discriminator(handlerType))
                        {
                            var subscriptionName = String.Format("{0}.{1}", handlerType.GetBoundedContext().BoundedContextNamespace, processorSettings.MessageProcessorName);
                            messageSubscriptionMiddleware.Subscribe(new SubscriberMiddleware(subscriptionName, reg.Key, handlerType, projectionsMiddleware));
                        }
                    }
                }
                return processor;
            };
            builder.Container.RegisterSingleton<IMessageProcessor>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public class PortMessageProcessorSettings : SettingsBuilder, IMessageProcessorSettings<IEvent>
    {
        public PortMessageProcessorSettings(ISettingsBuilder builder, Func<Type, bool> discriminator) : base(builder)
        {
            this.discriminator = discriminator;
        }
        private Func<Type, bool> discriminator;

        Dictionary<Type, List<Type>> IMessageProcessorSettings<IEvent>.HandlerRegistrations { get; set; }

        Func<Type, object> IMessageProcessorSettings<IEvent>.HandlerFactory { get; set; }

        string IMessageProcessorSettings<IEvent>.MessageProcessorName { get; set; }

        Middleware<MessageHandlerMiddleware.HandleContext> IMessageProcessorSettings<IEvent>.ActualHandle { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as IMessageProcessorSettings<IEvent>;
            Func<IMessageProcessor> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);
                var publisher = builder.Container.Resolve<IPublisher<ICommand>>(builder.Name);

                var portsMiddleware = new PortsMiddleware(handlerFactory, publisher);
                var actualHandleHook = (this as IMessageProcessorSettings<IEvent>).ActualHandle;
                if (ReferenceEquals(null, actualHandleHook))
                    portsMiddleware.ActualHandle.Use(actualHandleHook);

                var messageSubscriptionMiddleware = new MessageSubscriptionsMiddleware();
                IMessageProcessor processor = new CronusMessageProcessorMiddleware(processorSettings.MessageProcessorName, messageSubscriptionMiddleware);

                foreach (var reg in (this as IMessageProcessorSettings<IEvent>).HandlerRegistrations)
                {
                    foreach (var handlerType in reg.Value)
                    {
                        if (discriminator == null || discriminator(handlerType))
                        {
                            var subscriptionName = String.Format("{0}.{1}", handlerType.GetBoundedContext().BoundedContextNamespace, processorSettings.MessageProcessorName);
                            messageSubscriptionMiddleware.Subscribe(new SubscriberMiddleware(subscriptionName, reg.Key, handlerType, portsMiddleware));
                        }
                    }
                }
                return processor;
            };
            builder.Container.RegisterSingleton<IMessageProcessor>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public class ApplicationServiceMessageProcessorSettings : SettingsBuilder, IMessageProcessorSettings<ICommand>
    {
        Func<Type, bool> discriminator;

        public ApplicationServiceMessageProcessorSettings(ISettingsBuilder builder, Func<Type, bool> discriminator) : base(builder)
        {
            this.discriminator = discriminator;
        }

        Dictionary<Type, List<Type>> IMessageProcessorSettings<ICommand>.HandlerRegistrations { get; set; }

        Func<Type, object> IMessageProcessorSettings<ICommand>.HandlerFactory { get; set; }

        string IMessageProcessorSettings<ICommand>.MessageProcessorName { get; set; }

        Middleware<MessageHandlerMiddleware.HandleContext> IMessageProcessorSettings<ICommand>.ActualHandle { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as IMessageProcessorSettings<ICommand>;
            Func<IMessageProcessor> messageHandlerProcessorFactory = () =>
            {
                var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);
                var repository = builder.Container.Resolve<IAggregateRepository>(builder.Name);
                var publisher = builder.Container.Resolve<IPublisher<IEvent>>(builder.Name);

                //create extension methis UseApplicationMiddleware instead of instance here.
                var applicationServiceMiddleware = new ApplicationServiceMiddleware(handlerFactory, repository, publisher);
                var actualHandleHook = (this as IMessageProcessorSettings<ICommand>).ActualHandle;
                if (ReferenceEquals(null, actualHandleHook))
                    applicationServiceMiddleware.ActualHandle.Use(actualHandleHook);

                var messageSubscriptionMiddleware = new MessageSubscriptionsMiddleware();
                IMessageProcessor processor = new CronusMessageProcessorMiddleware(processorSettings.MessageProcessorName, messageSubscriptionMiddleware);

                foreach (var reg in (this as IMessageProcessorSettings<ICommand>).HandlerRegistrations)
                {
                    foreach (var handlerType in reg.Value)
                    {
                        if (discriminator == null || discriminator(handlerType))
                        {
                            var subscriptionName = String.Format("{0}.{1}", handlerType.GetBoundedContext().BoundedContextNamespace, processorSettings.MessageProcessorName);
                            messageSubscriptionMiddleware.Subscribe(new SubscriberMiddleware(subscriptionName, reg.Key, handlerType, applicationServiceMiddleware));
                        }
                    }
                }
                return processor;
            };
            builder.Container.RegisterSingleton<IMessageProcessor>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public static class MessageProcessorWithSafeBatchSettingsExtensions
    {
        public static T UseProjections<T>(this T self, Action<ProjectionMessageProcessorSettings> configure) where T : IConsumerSettings<IEvent>
        {
            ProjectionMessageProcessorSettings settings = new ProjectionMessageProcessorSettings(self, t => typeof(IProjection).IsAssignableFrom(t));
            (settings as IMessageProcessorSettings<IEvent>).MessageProcessorName = "Projections";
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UsePorts<T>(this T self, Action<PortMessageProcessorSettings> configure) where T : PortConsumerSettings
        {
            PortMessageProcessorSettings settings = new PortMessageProcessorSettings(self, t => typeof(IPort).IsAssignableFrom(t));
            (settings as IMessageProcessorSettings<IEvent>).MessageProcessorName = "Ports";
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseApplicationServices<T>(this T self, Action<ApplicationServiceMessageProcessorSettings> configure) where T : IConsumerSettings<ICommand>
        {
            ApplicationServiceMessageProcessorSettings settings = new ApplicationServiceMessageProcessorSettings(self, t => typeof(IAggregateRootApplicationService).IsAssignableFrom(t));
            (settings as IMessageProcessorSettings<ICommand>).MessageProcessorName = "Commands";
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseApplicationServiceMiddleware<T>(this T self, Action<ApplicationServiceMessageProcessorSettings> configure) where T : IConsumerSettings<ICommand>
        {
            ApplicationServiceMessageProcessorSettings settings = new ApplicationServiceMessageProcessorSettings(self, t => typeof(IAggregateRootApplicationService).IsAssignableFrom(t));
            (settings as IMessageProcessorSettings<ICommand>).MessageProcessorName = "Commands";
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }
    }
}
