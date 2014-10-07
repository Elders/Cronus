using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.UnitOfWork;
using Elders.Cronus.IocContainer;

namespace Elders.Cronus.Pipeline.Config
{
    public class MessageProcessorSettings<TContract> : IMessageProcessorSettings<TContract>, IHaveUnitOfWorkFactory where TContract : IMessage
    {
        public MessageProcessorSettings()
        {
            (this as IHaveUnitOfWorkFactory).UnitOfWorkFactory = new Lazy<UnitOfWorkFactory>(() => new UnitOfWorkFactory());
        }

        Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> IMessageProcessorSettings<TContract>.HandlerRegistrations { get; set; }

        Lazy<UnitOfWorkFactory> IHaveUnitOfWorkFactory.UnitOfWorkFactory { get; set; }

        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        void ISettingsBuilder.Build()
        {
            var builder = this as ISettingsBuilder;

            var messageHandlerProcessor = new Lazy<IMessageProcessor<IEvent>>(() =>
            {
                var safeBatchFactory = new SafeBatchWithBatchUnitOfWorkContextFactory<TransportMessage>((this as IHaveUnitOfWorkFactory).UnitOfWorkFactory.Value);

                var handler = new MessageHandlerCollection<IEvent>(safeBatchFactory);

                foreach (var reg in (this as IMessageProcessorSettings<TContract>).HandlerRegistrations)
                {
                    foreach (var item in reg.Value)
                    {
                        if (!typeof(IPort).IsAssignableFrom(item.Item1))
                            handler.RegisterHandler(reg.Key, item.Item1, item.Item2);
                    }
                }
                return handler;
            });
        }
    }

    public static class MessageProcessorWithSafeBatchSettingsExtensions
    {
        public static T UseUnitOfWork<T>(this T self, UnitOfWorkFactory instance) where T : IHaveUnitOfWorkFactory
        {
            self.UnitOfWorkFactory = new Lazy<UnitOfWorkFactory>(() => instance);

            return self;
        }

        public static T UseProjections<T>(this T self, Action<MessageProcessorSettings<IEvent>> configure) where T : IMessageProcessorSettings<IEvent>
        {
            MessageProcessorSettings<IEvent> settings = new MessageProcessorSettings<IEvent>();
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UsePortsAndProjections<T>(this T self, Action<MessageProcessorSettings<IEvent>> configure) where T : IMessageProcessorSettings<IEvent>
        {
            MessageProcessorSettings<IEvent> settings = new MessageProcessorSettings<IEvent>();
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UsePorts<T>(this T self, Action<MessageProcessorSettings<IEvent>> configure) where T : IMessageProcessorSettings<IEvent>
        {
            MessageProcessorSettings<IEvent> settings = new MessageProcessorSettings<IEvent>();
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseApplicationServices<T>(this T self, Action<MessageProcessorSettings<ICommand>> configure) where T : IMessageProcessorSettings<ICommand>
        {
            MessageProcessorSettings<ICommand> settings = new MessageProcessorSettings<ICommand>();
            if (configure != null)
                configure(settings);

            (settings as ISettingsBuilder).Build();
            return self;
        }
    }
}