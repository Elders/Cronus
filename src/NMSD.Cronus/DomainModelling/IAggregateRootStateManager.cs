using System.Collections.Generic;

namespace NMSD.Cronus.DomainModelling
{
    public interface IAggregateRootStateManager
    {
        IAggregateRootState State { get; set; }
        IAggregateRootState BuildStateFromHistory(List<IEvent> events);
    }
}