using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cronus.Core.Eventing
{
    public abstract class AbstractEventBus : IEventBus
    {
        protected Dictionary<Type, List<Func<IEvent, bool>>> handlers = new Dictionary<Type, List<Func<IEvent, bool>>>();

        public abstract bool Publish(IEvent @event);

        public abstract Task<bool> PublishAsync(IEvent @event);

        public void RegisterEventHandler(Type eventType, Type eventHandlerType, Func<Type, IEventHandler> eventHandlerFactory)
        {
            if (!handlers.ContainsKey(eventType))
            {
                handlers[eventType] = new List<Func<IEvent, bool>>();
            }

            handlers[eventType].Add(x => Handle(x, eventHandlerType, eventHandlerFactory));
        }

        bool Handle(IEvent @event, Type eventHandlerType, Func<Type, IEventHandler> eventHandlerFactory)
        {
            dynamic handler = null;
            try
            {
                handler = eventHandlerFactory(eventHandlerType);
                onHandleEvent(@event, handler);
                handler.Handle((dynamic)@event);
                onEventHandled(@event, handler);
                return true;
            }
            catch (Exception ex)
            {
                onErrorHandlingEvent(@event, handler, ex);
                return false;
            }
        }

        Action<IEvent> onEventPublished = (x => { });

        Action<IEvent> onPublishEvent = (x => { });

        Action<IEvent, IEventHandler> onHandleEvent = (x, y) => { };

        Action<IEvent, IEventHandler> onEventHandled = (x, y) => { };

        Action<IEvent, IEventHandler, Exception> onErrorHandlingEvent = (x, y, z) => { };

        public void OnEventPublished(Action<IEvent> action)
        {
            onEventPublished = action;
        }

        public void OnPublishEvent(Action<IEvent> action)
        {
            onPublishEvent = action;
        }

        public void OnHandleEvent(Action<IEvent, IEventHandler> action)
        {
            onHandleEvent = action;
        }

        public void OnEventHandled(Action<IEvent, IEventHandler> action)
        {
            onEventHandled = action;
        }

        public void OnErrorHandlingEvent(Action<IEvent, IEventHandler, Exception> action)
        {
            onErrorHandlingEvent = action;
        }
    }
}