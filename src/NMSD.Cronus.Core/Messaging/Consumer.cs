using System;
using System.Linq;
using System.Collections.Generic;

namespace NMSD.Cronus.Core.Messaging
{
    public abstract class Consumer<TMessage, TMessageHandler> : IConsumer<TMessageHandler>
        where TMessage : IMessage
        where TMessageHandler : IMessageHandler
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Consumer<TMessage, TMessageHandler>));

        protected Dictionary<Type, Dictionary<Type, LateBoundVoidMethod>> handlerCallbacks = new Dictionary<Type, Dictionary<Type, LateBoundVoidMethod>>();

        protected Dictionary<Type, List<Func<TMessage, bool>>> handlers = new Dictionary<Type, List<Func<TMessage, bool>>>();

        public virtual void RegisterHandler(Type messageType, Type messageHandlerType, Func<Type, TMessageHandler> handlerFactory)
        {
            if (!handlers.ContainsKey(messageType))
                handlers[messageType] = new List<Func<TMessage, bool>>();

            handlers[messageType].Add(x => Handle(x, messageHandlerType, handlerFactory));

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

        public abstract void Start(int numberOfWorkers);

        public abstract void Stop();

        protected bool Handle(TMessage message)
        {
            List<Func<TMessage, bool>> availableHandlers;
            if (handlers.TryGetValue(message.GetType(), out availableHandlers))
            {
                foreach (var handleMethod in availableHandlers)
                {
                    var result = handleMethod(message);
                    if (result == false)
                        return result;
                }
            }
            return true;
        }

        protected bool Handle(TMessage message, Type eventHandlerType, Func<Type, TMessageHandler> handlerFactory)
        {
            try
            {
                var handler = handlerFactory(eventHandlerType);
                handlerCallbacks[eventHandlerType][message.GetType()](handler, new object[] { message });
                log.Info("HANDLE => " + message.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}