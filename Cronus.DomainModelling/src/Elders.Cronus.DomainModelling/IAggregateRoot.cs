using System.Collections.Generic;

namespace Elders.Cronus.DomainModelling
{
    public interface IAggregateRoot : IAggregateRootStateManager
    {
        void Apply(IEvent @event);
        List<IEvent> UncommittedEvents { get; set; }
    }
}