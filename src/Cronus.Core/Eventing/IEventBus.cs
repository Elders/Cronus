using System;
using System.Threading.Tasks;

namespace Cronus.Core.Eventing
{
    public interface IEventBus
    {
        void RegisterEventHandler(Type eventType, Type eventHandlerType, Func<Type, IEventHandler> eventHandlerFactory);
        bool Publish(IEvent @event);
        Task<bool> PublishAsync(IEvent @event);
        void OnEventPublished(Action<IEvent> action);
        void OnPublishEvent(Action<IEvent> action);
        void OnHandleEvent(Action<IEvent, IEventHandler> action);
        void OnEventHandled(Action<IEvent, IEventHandler> action);
        void OnErrorHandlingEvent(Action<IEvent, IEventHandler, Exception> action);
    }
}