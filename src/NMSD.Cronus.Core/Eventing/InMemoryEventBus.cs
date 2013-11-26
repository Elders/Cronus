using Cronus.Core.Eventing;
using NMSD.Cronus.Core.Publishing;
using NMSD.Cronus.Core.Snapshotting;

namespace NMSD.Cronus.Core.Eventing
{
    public class InMemoryEventBus : InMemoryPublisher<IEvent, IEventHandler>
    {

    }
}
