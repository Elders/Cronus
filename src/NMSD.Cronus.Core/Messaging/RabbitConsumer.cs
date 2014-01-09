using System;
using System.Linq;
using System.Collections.Generic;

namespace NMSD.Cronus.Core.Messaging
{
    public abstract class RabbitConsumer<TMessage, TMessageHandler> : BaseInMemoryConsumer<TMessage, TMessageHandler>
        where TMessage : IMessage
        where TMessageHandler : IMessageHandler
    {
        protected QueueFactory queueFactory;

        public override void RegisterHandler(Type messageType, Type messageHandlerType, Func<Type, TMessageHandler> handlerFactory)
        {
            base.RegisterHandler(messageType, messageHandlerType, handlerFactory);
            queueFactory.Register(messageType, messageHandlerType);
        }
    }
}
