using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Messaging.MessageHandleScope;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Pipeline.Config
{
    public class MessageProcessorWithSafeBatchSettings<TContract> : IMessageProcessorWithSafeBatchSettings<TContract>, IHaveScopeFactory where TContract : IMessage
    {
        public MessageProcessorWithSafeBatchSettings()
        {
            (this as IHaveScopeFactory).ScopeFactory = new Lazy<ScopeFactory>(() => new ScopeFactory());
        }

        Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> IMessageProcessorWithSafeBatchSettings<TContract>.HandlerRegistrations { get; set; }

        Lazy<ScopeFactory> IHaveScopeFactory.ScopeFactory { get; set; }
    }

    public class EventStoreMessageProcessorWithSafeBatchSettings : IMessageProcessorWithSafeBatchSettings<DomainMessageCommit>
    {
        Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> IMessageProcessorWithSafeBatchSettings<DomainMessageCommit>.HandlerRegistrations { get; set; }
    }

    public static class MessageProcessorWithSafeBatchSettingsExtensions
    {
        public static T UseScopeFactory<T>(this T self, ScopeFactory instance) where T : IHaveScopeFactory
        {
            self.ScopeFactory = new Lazy<ScopeFactory>(() => instance);

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
                var safeBatchFactory = new SafeBatchWithBatchScopeContextFactory<TransportMessage>((settings as IHaveScopeFactory).ScopeFactory.Value);

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

        public static T UsePortHandler<T>(this T self, Action<MessageProcessorWithSafeBatchSettings<IEvent>> configure) where T : IHaveMessageProcessor<IEvent>
        {
            MessageProcessorWithSafeBatchSettings<IEvent> settings = new MessageProcessorWithSafeBatchSettings<IEvent>();
            if (configure != null)
                configure(settings);

            var castedSettings = settings as IMessageProcessorWithSafeBatchSettings<IEvent>;


            self.MessageHandlerProcessor = new Lazy<IMessageProcessor<IEvent>>(() =>
            {
                var safeBatchFactory = new SafeBatchWithBatchScopeContextFactory<TransportMessage>((settings as IHaveScopeFactory).ScopeFactory.Value);

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
                var safeBatchFactory = new SafeBatchWithBatchScopeContextFactory<TransportMessage>((settings as IHaveScopeFactory).ScopeFactory.Value);

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