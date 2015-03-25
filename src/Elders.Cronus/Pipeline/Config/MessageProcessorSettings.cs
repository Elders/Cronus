using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline.Config
{
    public class ProjectionMessageProcessorSettings : SettingsBuilder, IMessageProcessorSettings<IEvent>
    {
        public ProjectionMessageProcessorSettings(ISettingsBuilder builder, Func<Type, bool> discriminator) : base(builder)
        {
            this.discriminator = discriminator;
        }
        private Func<Type, bool> discriminator;

        Dictionary<Type, List<Tuple<Type, Func<Type, object>>>> IMessageProcessorSettings<IEvent>.HandlerRegistrations { get; set; }

        string IMessageProcessorSettings<IEvent>.MessageProcessorName { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as IMessageProcessorSettings<IEvent>;
            Func<IMessageProcessor> messageHandlerProcessorFactory = () =>
            {
                IMessageProcessor handler = new MessageProcessor(processorSettings.MessageProcessorName, builder.Container);

                foreach (var reg in (this as IMessageProcessorSettings<IEvent>).HandlerRegistrations)
                {
                    foreach (var item in reg.Value)
                    {
                        if (discriminator == null || discriminator(item.Item1))
                        {
                            var handlerFactory = new DefaultHandlerFactory(item.Item1, item.Item2);
                            handler.Subscribe(new ProjectionSubscription(reg.Key, handlerFactory));
                        }
                    }
                }
                return handler;
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

        Dictionary<Type, List<Tuple<Type, Func<Type, object>>>> IMessageProcessorSettings<IEvent>.HandlerRegistrations { get; set; }

        string IMessageProcessorSettings<IEvent>.MessageProcessorName { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as IMessageProcessorSettings<IEvent>;
            Func<IMessageProcessor> messageHandlerProcessorFactory = () =>
            {
                IMessageProcessor handler = new MessageProcessor(processorSettings.MessageProcessorName, builder.Container);

                foreach (var reg in (this as IMessageProcessorSettings<IEvent>).HandlerRegistrations)
                {
                    foreach (var item in reg.Value)
                    {
                        if (discriminator == null || discriminator(item.Item1))
                        {
                            var handlerFactory = new DefaultHandlerFactory(item.Item1, item.Item2);
                            var publisher = builder.Container.Resolve<IPublisher<ICommand>>(builder.Name);
                            handler.Subscribe(new PortSubscription(reg.Key, handlerFactory, publisher));
                        }
                    }
                }
                return handler;
            };
            builder.Container.RegisterSingleton<IMessageProcessor>(() => messageHandlerProcessorFactory(), builder.Name);
        }
    }

    public class ApplicationServiceMessageProcessorSettings : SettingsBuilder, IMessageProcessorSettings<ICommand>
    {
        public ApplicationServiceMessageProcessorSettings(ISettingsBuilder builder, Func<Type, bool> discriminator) : base(builder)
        {
            this.discriminator = discriminator;
        }
        private Func<Type, bool> discriminator;

        Dictionary<Type, List<Tuple<Type, Func<Type, object>>>> IMessageProcessorSettings<ICommand>.HandlerRegistrations { get; set; }

        string IMessageProcessorSettings<ICommand>.MessageProcessorName { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            var processorSettings = this as IMessageProcessorSettings<ICommand>;
            Func<IMessageProcessor> messageHandlerProcessorFactory = () =>
            {
                IMessageProcessor handler = new MessageProcessor(processorSettings.MessageProcessorName, builder.Container);

                foreach (var reg in (this as IMessageProcessorSettings<ICommand>).HandlerRegistrations)
                {
                    foreach (var item in reg.Value)
                    {
                        if (discriminator == null || discriminator(item.Item1))
                        {
                            var handlerFactory = new DefaultHandlerFactory(item.Item1, item.Item2);
                            var repository = builder.Container.Resolve<IAggregateRepository>(builder.Name);
                            var publisher = builder.Container.Resolve<IPublisher<IEvent>>(builder.Name);
                            handler.Subscribe(new ApplicationServiceSubscription(reg.Key, handlerFactory, repository, publisher));
                        }
                    }
                }
                return handler;
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
            (settings as IMessageProcessorSettings<IEvent>).MessageProcessorName = "Commands";
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }
    }
}
