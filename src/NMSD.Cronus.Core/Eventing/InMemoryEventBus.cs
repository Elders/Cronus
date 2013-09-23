using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cronus.Core.Eventing
{
    /// <summary>
    /// Represents an in memory event messaging destribution
    /// </summary>
    public class InMemoryEventBus : AbstractEventBus
    {
        /// <summary>
        /// Publishes the given event to all registered event handlers
        /// </summary>
        /// <param name="event">An event instance</param>
        public override bool Publish(IEvent @event)
        {
            onPublishEvent(@event);
            List<Func<IEvent, bool>> eventHandlers;
            if (handlers.TryGetValue(@event.GetType(), out eventHandlers))
            {
                foreach (var handleMethod in eventHandlers)
                {
                    var result = handleMethod(@event);
                    if (result == false)
                        return result;
                }
            }
            onEventPublished(@event);
            return true;
        }

        public override Task<bool> PublishAsync(IEvent @event)
        {
            return Threading.RunAsync(() => Publish(@event));
        }
    }
}