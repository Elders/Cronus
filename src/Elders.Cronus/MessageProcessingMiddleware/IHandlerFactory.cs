using System;

namespace Elders.Cronus.MessageProcessingMiddleware
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

    public interface IHandlerInstance : IDisposable
    {
        object Current { get; }
    }

    public class DefaultHandlerInstance : IHandlerInstance
    {
        public DefaultHandlerInstance(object instance)
        {
            Current = instance;
        }

        public object Current { get; set; }

        public void Dispose()
        {
            var disposeMe = Current as IDisposable;
            if (ReferenceEquals(null, disposeMe) == false)
                disposeMe.Dispose();
        }
    }
}
