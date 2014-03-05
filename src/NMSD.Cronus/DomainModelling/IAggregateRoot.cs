using System.Collections.Generic;

namespace NMSD.Cronus.DomainModelling
{
    public interface IAggregateRoot : IAggregateRootStateManager
    {
        void Apply(IEvent @event);
        List<IEvent> UncommittedEvents { get; set; }
    }
}