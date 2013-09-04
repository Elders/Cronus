using Cronus.Core.Eventing;

namespace Cronus.Core.EventStore
{
    /// <summary>
    /// Indicates the ability to store and retreive a stream of events.
    /// </summary>
    /// <remarks>
    /// Instances of this class must be designed to be multi-thread safe such that they can be shared between threads.
    /// </remarks>
    public interface IEventStore
    {
        /// <summary>
        /// Stores an event
        /// </summary>
        /// <param name="event"></param>
        void Save(IEvent @event);
    }
}
