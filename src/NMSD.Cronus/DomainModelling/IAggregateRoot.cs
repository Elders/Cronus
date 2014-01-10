using System.Collections.Generic;
using NMSD.Cronus.Eventing;

namespace NMSD.Cronus.DomainModelling
{
    public interface IAggregateRoot : IAggregateRootStateManager
    {
        void Apply(IEvent @event);
        List<IEvent> UncommittedEvents { get; set; }
    }
}