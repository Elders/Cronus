using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cronus.Core.Eventing
{
    /// <summary>
    /// Represents an in memory event messaging destribution
    /// </summary>
    public class InMemoryEventBusAsync : IEventBus
    {
        Dictionary<Type, List<Func<IEvent, Task<bool>>>> handlers = new Dictionary<Type, List<Func<IEvent, Task<bool>>>>();

        public void RegisterEventHandler(Type eventType, Type eventHandlerType, Func<Type, IEventHandler> eventHandlerFactory)
        {
            if (!handlers.ContainsKey(eventType))
            {
                handlers[eventType] = new List<Func<IEvent, Task<bool>>>();
            }

            handlers[eventType].Add(x => RunAsync(() => Handle(x, eventHandlerType, eventHandlerFactory)));
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

        Task<T> RunAsync<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();

            ThreadPool.QueueUserWorkItem(delegate
            {
                try { tcs.SetResult(func()); }
                catch (Exception ex) { tcs.SetException(ex); }
            });

            return tcs.Task;
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

            Console.WriteLine("all published");
        }
    }
}