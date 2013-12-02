using System;

namespace NMSD.Cronus.Core.Messaging
{
    public interface IConsumer<THandler>
    {
        void RegisterHandler(Type eventType, Type eventHandlerType, Func<Type, THandler> handlerFactory);

        //void Start();
    }
}