using System.Collections.Generic;
using NMSD.Cronus.Eventing;

namespace NMSD.Cronus.DomainModelling
{
    public interface IAggregateRootStateManager
    {
        IAggregateRootState State { get; set; }
        IAggregateRootState BuildStateFromHistory(List<IEvent> events);
    }
}