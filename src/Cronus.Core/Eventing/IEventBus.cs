using System;
using System.Threading.Tasks;

namespace Cronus.Core.Eventing
{
    public interface IEventBus
    {
        void RegisterEventHandler(Type eventType, Type eventHandlerType, Func<Type, IEventHandler> eventHandlerFactory);
        bool Publish(IEvent @event);
        Task<bool> PublishAsync(IEvent @event);
    }
}