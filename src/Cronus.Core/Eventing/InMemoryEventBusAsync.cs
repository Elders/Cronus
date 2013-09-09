using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cronus.Core.Eventing
{
    /// <summary>
    /// Represents an in memory event messaging destribution
    /// </summary>
    public class InMemoryEventBusAsync : AbstractEventBus
    {
        /// <summary>
        /// Publishes the given event to all registered event handlers
        /// </summary>
        /// <param name="event">An event instance</param>
        public override bool Publish(IEvent @event)
        {
            onPublishEvent(@event);
            List<Task<bool>> tasks = new List<Task<bool>>();
            foreach (var handleMethod in handlers[@event.GetType()])
            {
                tasks.Add(Threading.RunAsync(() => handleMethod(@event)));
            }
            Task.WaitAll(tasks.ToArray());
            onEventPublished(@event);
            return true;
        }

        public override Task<bool> PublishAsync(IEvent @event)
        {
            return Threading.RunAsync(() => Publish(@event));
        }

    }
}