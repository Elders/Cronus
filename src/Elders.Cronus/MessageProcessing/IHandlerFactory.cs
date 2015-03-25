using System;

namespace Elders.Cronus.MessageProcessing
{
    public interface IHandlerFactory
    {
        Type MessageHandlerType { get; }
        object Create();
    }

    public class DefaultHandlerFactory : IHandlerFactory
    {
        private readonly Func<Type, object> handlerFctory;

        public DefaultHandlerFactory(Type messageHandlerType, Func<Type, object> handlerFactory)
        {
            MessageHandlerType = messageHandlerType;
            this.handlerFctory = handlerFactory;
        }

        public Type MessageHandlerType { get; private set; }

        public object Create()
        {
            return handlerFctory(MessageHandlerType);
        }
    }
}
