using System;

namespace Elders.Cronus.MessageProcessingMiddleware
{
    public interface IHandlerFactory
    {
        IHandlerInstance Create(Type handlerType);
    }

    public class DefaultHandlerFactory : IHandlerFactory
    {
        readonly Func<Type, object> handlerFctory;

        public DefaultHandlerFactory(Func<Type, object> handlerFactory)
        {
            this.handlerFctory = handlerFactory;
        }


        public IHandlerInstance Create(Type handlerType)
        {
            return new DefaultHandlerInstance(handlerFctory(handlerType));
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
