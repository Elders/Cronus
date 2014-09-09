using System.Collections.Generic;

namespace Elders.Cronus.DomainModeling
{
    public interface IAggregateRootStateManager
    {
        IAggregateRootState State { get; set; }
        IAggregateRootState BuildStateFromHistory(List<IEvent> events);
    }
}