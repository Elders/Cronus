using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus.Pipeline.Config
{
    public class MessageProcessorWithSafeBatchSettings<TContract> : IMessageProcessorWithSafeBatchSettings<TContract>, IHaveUnitOfWorkFactory where TContract : IMessage
    {
        public MessageProcessorWithSafeBatchSettings()
        {
            (this as IHaveUnitOfWorkFactory).UnitOfWorkFactory = new Lazy<UnitOfWorkFactory>(() => new UnitOfWorkFactory());
        }

        Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> IMessageProcessorWithSafeBatchSettings<TContract>.HandlerRegistrations { get; set; }

        Lazy<UnitOfWorkFactory> IHaveUnitOfWorkFactory.UnitOfWorkFactory { get; set; }
    }

    public static class MessageProcessorWithSafeBatchSettingsExtensions
    {
        public static T UseUnitOfWork<T>(this T self, UnitOfWorkFactory instance) where T : IHaveUnitOfWorkFactory
        {
            self.UnitOfWorkFactory = new Lazy<UnitOfWorkFactory>(() => instance);

            return self;
        }

        public static T UseEventHandler<T>(this T self, Action<MessageProcessorWithSafeBatchSettings<IEvent>> configure) where T : IHaveMessageProcessor<IEvent>
        {
            MessageProcessorWithSafeBatchSettings<IEvent> settings = new MessageProcessorWithSafeBatchSettings<IEvent>();
            if (configure != null)
                configure(settings);

            var castedSettings = settings as IMessageProcessorWithSafeBatchSettings<IEvent>;


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

        public static T UseInMemoryPortAndEventHandler<T>(this T self, Action<MessageProcessorWithSafeBatchSettings<IEvent>> configure) where T : IHaveMessageProcessor<IEvent>
        {
            MessageProcessorWithSafeBatchSettings<IEvent> settings = new MessageProcessorWithSafeBatchSettings<IEvent>();
            if (configure != null)
                configure(settings);

            var castedSettings = settings as IMessageProcessorWithSafeBatchSettings<IEvent>;


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

        public static T UsePortHandler<T>(this T self, Action<MessageProcessorWithSafeBatchSettings<IEvent>> configure) where T : IHaveMessageProcessor<IEvent>
        {
            MessageProcessorWithSafeBatchSettings<IEvent> settings = new MessageProcessorWithSafeBatchSettings<IEvent>();
            if (configure != null)
                configure(settings);

            var castedSettings = settings as IMessageProcessorWithSafeBatchSettings<IEvent>;


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

        public static T UseCommandHandler<T>(this T self, Action<MessageProcessorWithSafeBatchSettings<ICommand>> configure) where T : IHaveMessageProcessor<ICommand>
        {
            MessageProcessorWithSafeBatchSettings<ICommand> settings = new MessageProcessorWithSafeBatchSettings<ICommand>();
            if (configure != null)
                configure(settings);

            var castedSettings = settings as IMessageProcessorWithSafeBatchSettings<ICommand>;

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