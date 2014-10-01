using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.UnitOfWork;
using Elders.Cronus.Pipeline.Config;

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
    }

    public static class MessageProcessorWithSafeBatchSettingsExtensions
    {
        public static T UseUnitOfWork<T>(this T self, UnitOfWorkFactory instance) where T : IHaveUnitOfWorkFactory
        {
            self.UnitOfWorkFactory = new Lazy<UnitOfWorkFactory>(() => instance);

            return self;
        }

        public static T UseProjections<T>(this T self, Action<MessageProcessorSettings<IEvent>> configure) where T : IHaveMessageProcessor<IEvent>
        {
            MessageProcessorSettings<IEvent> settings = new MessageProcessorSettings<IEvent>();
            if (configure != null)
                configure(settings);

            var castedSettings = settings as IMessageProcessorSettings<IEvent>;


            self.MessageHandlerProcessor = new Lazy<IMessageProcessor<IEvent>>(() =>
            {
                var safeBatchFactory = new SafeBatchWithBatchUnitOfWorkContextFactory<TransportMessage>((settings as IHaveUnitOfWorkFactory).UnitOfWorkFactory.Value);

                var handler = new MessageHandlerCollection<IEvent>(safeBatchFactory);

                foreach (var reg in castedSettings.HandlerRegistrations)
                {
                    foreach (var item in reg.Value)
                    {
                        if (!typeof(IPort).IsAssignableFrom(item.Item1))
                            handler.RegisterHandler(reg.Key, item.Item1, item.Item2);
                    }
                }
                return handler;
            });
            return self;
        }

        public static T UsePortsAndProjections<T>(this T self, Action<MessageProcessorSettings<IEvent>> configure) where T : IHaveMessageProcessor<IEvent>
        {
            MessageProcessorSettings<IEvent> settings = new MessageProcessorSettings<IEvent>();
            if (configure != null)
                configure(settings);

            var castedSettings = settings as IMessageProcessorSettings<IEvent>;


            self.MessageHandlerProcessor = new Lazy<IMessageProcessor<IEvent>>(() =>
            {
                var safeBatchFactory = new SafeBatchWithBatchUnitOfWorkContextFactory<TransportMessage>((settings as IHaveUnitOfWorkFactory).UnitOfWorkFactory.Value);

                var handler = new MessageHandlerCollection<IEvent>(safeBatchFactory);

                foreach (var reg in castedSettings.HandlerRegistrations)
                {
                    foreach (var item in reg.Value)
                    {
                        handler.RegisterHandler(reg.Key, item.Item1, item.Item2);
                    }
                }
                return handler;
            });
            return self;
        }

        public static T UsePorts<T>(this T self, Action<MessageProcessorSettings<IEvent>> configure) where T : IHaveMessageProcessor<IEvent>
        {
            MessageProcessorSettings<IEvent> settings = new MessageProcessorSettings<IEvent>();
            if (configure != null)
                configure(settings);

            var castedSettings = settings as IMessageProcessorSettings<IEvent>;


            self.MessageHandlerProcessor = new Lazy<IMessageProcessor<IEvent>>(() =>
            {
                var safeBatchFactory = new SafeBatchWithBatchUnitOfWorkContextFactory<TransportMessage>((settings as IHaveUnitOfWorkFactory).UnitOfWorkFactory.Value);

                var handler = new MessageHandlerCollection<IEvent>(safeBatchFactory);

                foreach (var reg in castedSettings.HandlerRegistrations)
                {
                    foreach (var item in reg.Value)
                    {
                        if (typeof(IPort).IsAssignableFrom(item.Item1))
                            handler.RegisterHandler(reg.Key, item.Item1, item.Item2);
                    }
                }
                return handler;
            });
            return self;
        }

        public static T UseApplicationServices<T>(this T self, Action<MessageProcessorSettings<ICommand>> configure) where T : IHaveMessageProcessor<ICommand>
        {
            MessageProcessorSettings<ICommand> settings = new MessageProcessorSettings<ICommand>();
            if (configure != null)
                configure(settings);

            var castedSettings = settings as IMessageProcessorSettings<ICommand>;

            self.MessageHandlerProcessor = new Lazy<IMessageProcessor<ICommand>>(() =>
            {
                var safeBatchFactory = new SafeBatchWithBatchUnitOfWorkContextFactory<TransportMessage>((settings as IHaveUnitOfWorkFactory).UnitOfWorkFactory.Value);

                var handler = new MessageHandlerCollection<ICommand>(safeBatchFactory);

                foreach (var reg in castedSettings.HandlerRegistrations)
                {
                    foreach (var item in reg.Value)
                    {
                        handler.RegisterHandler(reg.Key, item.Item1, item.Item2);
                    }
                }
                return handler;
            });
            return self;
        }
    }
}