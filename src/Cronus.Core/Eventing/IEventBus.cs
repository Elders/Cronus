using System;

namespace Cronus.Core.Eventing
{
    public interface IEventBus
    {
        void RegisterEventHandler(Type eventType, Type eventHandlerType, Func<Type, IEventHandler> eventHandlerFactory);
        void Publish(IEvent @event);
    }
}