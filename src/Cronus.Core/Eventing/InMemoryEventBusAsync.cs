using System;
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
            foreach (var handleMethod in handlers[@event.GetType()])
            {
                Threading.RunAsync(() => handleMethod(@event));
            }

            return true;
        }

        public override Task<bool> PublishAsync(IEvent @event)
        {
            throw new NotImplementedException();
        }

    }
}