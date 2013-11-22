using Cronus.Core.Eventing;
using NMSD.Cronus.Core.Publishing;

namespace NMSD.Cronus.Core.Eventing
{
    public class InMemoryEventBus : InMemoryPublisher<IEvent, IEventHandler>
    {
    }
}
