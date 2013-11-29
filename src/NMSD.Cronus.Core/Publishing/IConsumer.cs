using System;

namespace NMSD.Cronus.Core.Publishing
{
    public interface IConsumer<THandler>
    {
        void RegisterHandler(Type eventType, Type eventHandlerType, Func<Type, THandler> handlerFactory);
    }
}