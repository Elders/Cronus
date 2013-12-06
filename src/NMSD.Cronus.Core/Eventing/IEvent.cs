using NMSD.Cronus.Core.Messaging;

namespace Cronus.Core.Eventing
{
    /// <summary>
    /// A markup interface telling that the implementing class is an event.
    /// </summary>
    public interface IEvent : IMessage { }
}