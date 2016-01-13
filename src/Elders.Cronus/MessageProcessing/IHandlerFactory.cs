using System;

namespace Elders.Cronus.MessageProcessing
{
    public interface IHandlerFactory
    {
        Type MessageHandlerType { get; }
        IHandlerInstance Create();
    }

    public class DefaultHandlerFactory : IHandlerFactory
    {
        readonly Func<Type, object> handlerFctory;

        public DefaultHandlerFactory(Type messageHandlerType, Func<Type, object> handlerFactory)
        {
            MessageHandlerType = messageHandlerType;
            this.handlerFctory = handlerFactory;
        }

        public Type MessageHandlerType { get; private set; }

        public IHandlerInstance Create()
        {
            return new DefaultHandlerInstance(handlerFctory(MessageHandlerType));
        }
    }
}
