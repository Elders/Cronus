using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            foreach (var handleMethod in handlers[@event.GetType()])
            {
                handleMethod(@event);
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
