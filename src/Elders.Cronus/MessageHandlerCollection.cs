using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus
{
    public class MessageHandlerCollection<TContract> : IMessageProcessor<TContract>
        where TContract : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MessageHandlerCollection<TContract>));

        //  Each handler can handle different contracts/messages where the actual handler body is LateBoundVoidMethod
        protected Dictionary<Type, Dictionary<Type, LateBoundVoidMethod>> handlerCallbacks = new Dictionary<Type, Dictionary<Type, LateBoundVoidMethod>>();

        //  Each contract/message is bound to an actual handle implementation and the reason we do it like this is because of the handlerFactory
        private Dictionary<Type, List<Func<TContract, Context, bool>>> handlers = new Dictionary<Type, List<Func<TContract, Context, bool>>>();

        //  Each contract/message can have several handler types/classes
        protected Dictionary<Type, HashSet<Type>> registeredHandlers = new Dictionary<Type, HashSet<Type>>();

        public MessageHandlerCollection(SafeBatchWithBatchUnitOfWorkContextFactory<TransportMessage> safeBatchFactory)
        {
            this.safeBatchFactory = safeBatchFactory;
        }

        public UnitOfWorkFactory UnitOfWorkFactory { get; private set; }

        public IEnumerable<Type> GetRegisteredHandlers()
        {
            return registeredHandlers.Keys.AsEnumerable();
        }

        public bool Handle(TContract message, Context context)
        {
            return safeBatchFactory.UnitOfWorkFactory.UseMessageUnitOfWork(context, (ctx) =>
            {
                List<Func<TContract, Context, bool>> availableHandlers;
                if (handlers.TryGetValue(message.GetType(), out availableHandlers))
                {
                    if (availableHandlers == null || availableHandlers.Count == 0)
                        log.WarnFormat("There is no handler for {0}", message);

                    foreach (var handleMethod in availableHandlers)
                    {
                        handleMethod(message, context);
                    }
                }
                return true;
            });
        }

        private readonly SafeBatchWithBatchUnitOfWorkContextFactory<TransportMessage> safeBatchFactory;

        public ISafeBatchResult<TransportMessage> Handle(List<TransportMessage> messages)
        {
            if (messages.Count == 0)
                throw new Exception("Do not pass empty collection of messages. PLEASE!");
            var safeBatch = safeBatchFactory.CreateSafeBatch();
            return safeBatch.Execute(messages, (msg, context) => Handle((TContract)msg.Payload, context));
        }

        public virtual void RegisterHandler(Type messageType, Type messageHandlerType, Func<Type, Context, object> handlerFactory)
        {
            if (!registeredHandlers.ContainsKey(messageHandlerType))
                registeredHandlers.Add(messageHandlerType, new HashSet<Type>());

            registeredHandlers[messageHandlerType].Add(messageType);

            if (!handlers.ContainsKey(messageType))
                handlers[messageType] = new List<Func<TContract, Context, bool>>();

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

        protected bool Handle(TContract message, Context context, Type eventHandlerType, Func<Type, Context, object> handlerFactory)
        {
            return safeBatchFactory.UnitOfWorkFactory.UseHandlerUnitOfWork(context, (ctx) =>
            {
                var handler = handlerFactory(eventHandlerType, ctx);
                if (log.IsInfoEnabled)
                    log.Info("HANDLE => " + eventHandlerType.Name + " => " + message);
                handlerCallbacks[eventHandlerType][message.GetType()](handler, new object[] { message });
                return true;
            });
        }
    }
}