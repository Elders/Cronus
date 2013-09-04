using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Cronus.Core.Eventing
{
    /// <summary>
    /// Represents an in memory event messaging destribution
    /// </summary>
    public class InMemoryEventBus : IEventBus
    {
        Dictionary<Type, List<Action<IEvent>>> handlers = new Dictionary<Type, List<Action<IEvent>>>();

        public void RegisterEventHandler(Type eventType, Type eventHandlerType, Func<Type, IEventHandler> eventHandlerFactory)
        {
            if (!handlers.ContainsKey(eventType))
            {
                handlers[eventType] = new List<Action<IEvent>>();
            }

            handlers[eventType].Add(x =>
            {
                dynamic handler = eventHandlerFactory(eventHandlerType);
                handler.Handle((dynamic)x);

            });
        }

        /// <summary>
        /// Publishes the given event to all registered event handlers
        /// </summary>
        /// <param name="event">An event instance</param>
        public void Publish(IEvent @event)
        {
            foreach (var handleMethod in handlers[@event.GetType()])
            {
                handleMethod(@event);
            }
        }
    }
}
