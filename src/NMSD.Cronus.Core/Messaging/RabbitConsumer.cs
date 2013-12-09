using System;
using System.Collections.Generic;

namespace NMSD.Cronus.Core.Messaging
{
    public class RabbitConsumer<TMessage, TMessageHandler> : IConsumer<TMessageHandler>
        where TMessage : IMessage
        where TMessageHandler : IMessageHandler
    {
        protected QueueFactory queueFactory;

        protected Dictionary<Type, List<Func<TMessage, bool>>> handlers = new Dictionary<Type, List<Func<TMessage, bool>>>();

        public void RegisterHandler(Type messageType, Type messageHandlerType, Func<Type, TMessageHandler> handlerFactory)
        {
            if (!handlers.ContainsKey(messageType))
                handlers[messageType] = new List<Func<TMessage, bool>>();

            handlers[messageType].Add(x => Handle(x, messageHandlerType, handlerFactory));

            queueFactory.Register(messageType, messageHandlerType);
        }

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
            dynamic handler = null;
            try
            {
                handler = handlerFactory(eventHandlerType);
                handler.Handle((dynamic)message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
