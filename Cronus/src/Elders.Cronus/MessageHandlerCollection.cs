using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus
{
    public class MessageHandlerCollection<TMessage> : IMessageProcessor<TMessage>
        where TMessage : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MessageHandlerCollection<TMessage>));

        //  Each handler can handle different contracts/messages where the actual handler body is LateBoundVoidMethod
        protected Dictionary<Type, Dictionary<Type, LateBoundVoidMethod>> handlerCallbacks = new Dictionary<Type, Dictionary<Type, LateBoundVoidMethod>>();

        //  Each contract/message is bound to an actual handle implementation and the reason we do it like this is because of the handlerFactory
        private Dictionary<Type, List<Func<TMessage, Context, bool>>> handlers = new Dictionary<Type, List<Func<TMessage, Context, bool>>>();

        //  Each contract/message can have several handler types/classes
        protected Dictionary<Type, HashSet<Type>> registeredHandlers = new Dictionary<Type, HashSet<Type>>();

        public MessageHandlerCollection(ScopeFactory scopeFactory, SafeBatchFactory<TMessage, Context> safeBatchFactory, int batchSize = 1)
        {
            this.safeBatchFactory = safeBatchFactory;
            BatchSize = batchSize;
            ScopeFactory = scopeFactory;
        }

        public int BatchSize { get; private set; }

        public ScopeFactory ScopeFactory { get; private set; }

        public IEnumerable<Type> GetRegisteredHandlers()
        {
            return registeredHandlers.Keys.AsEnumerable();
        }

        public bool Handle(TMessage message, Context context)
        {
            return ScopeFactory.UseMessageScope(context, (ctx) =>
            {
                List<Func<TMessage, Context, bool>> availableHandlers;
                if (handlers.TryGetValue(message.GetType(), out availableHandlers))
                {
                    foreach (var handleMethod in availableHandlers)
                    {
                        handleMethod(message, context);
                    }
                }
                return true;
            });
        }

        private readonly SafeBatchFactory<TMessage, Context> safeBatchFactory;

        public SafeBatchResult<TMessage> Handle(List<TMessage> messages)
        {
            if (messages.Count == 0)
                throw new Exception("Do not pass empty collection of messages. PLEASE!");
            var safeBatch = safeBatchFactory.Initialize();
            return safeBatch.Execute(messages, (msg, context) => Handle(msg, context));
        }

        public virtual void RegisterHandler(Type messageType, Type messageHandlerType, Func<Type, Context, object> handlerFactory)
        {
            if (!registeredHandlers.ContainsKey(messageHandlerType))
                registeredHandlers.Add(messageHandlerType, new HashSet<Type>());

            registeredHandlers[messageHandlerType].Add(messageType);

            if (!handlers.ContainsKey(messageType))
                handlers[messageType] = new List<Func<TMessage, Context, bool>>();

            handlers[messageType].Add((message, context) => Handle(message, context, messageHandlerType, handlerFactory));
            if (!handlerCallbacks.ContainsKey(messageHandlerType))
            {
                var callbacks = new Dictionary<Type, LateBoundVoidMethod>();
                var handleMethodInfos = messageHandlerType.GetMethods().Where(x => x.Name == "Handle");
                foreach (var mi in handleMethodInfos)
                {
                    var callback = DelegateFactory.Create(mi);
                    callbacks.Add(mi.GetParameters().First().ParameterType, callback);
                }
                handlerCallbacks.Add(messageHandlerType, callbacks);
            }
        }

        protected bool Handle(TMessage message, Context context, Type eventHandlerType, Func<Type, Context, object> handlerFactory)
        {
            return ScopeFactory.UseHandlerScope(context, (ctx) =>
            {
                var handler = handlerFactory(eventHandlerType, ctx);
                handlerCallbacks[eventHandlerType][message.GetType()](handler, new object[] { message });
                if (log.IsInfoEnabled)
                    log.Info("HANDLE => " + message);

                return true;
            });
        }
    }
}