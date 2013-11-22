using System;

namespace NMSD.Cronus.Core.Publishing
{
    public interface IPublisher<TMessage, THandler>
    {
        void RegisterHandler(Type eventType, Type eventHandlerType, Func<Type, THandler> handlerFactory);

        bool Publish(TMessage message);
    }
}