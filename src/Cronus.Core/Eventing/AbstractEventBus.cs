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
            try
            {
                dynamic handler = eventHandlerFactory(eventHandlerType);
                //onBeginHandle(handler);
                handler.Handle((dynamic)@event);
                //onEndHandle(handler)
                return true;
            }
            catch (Exception ex)
            {
                //onErrorHandle(@event,Handler)
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}