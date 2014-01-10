using System.Collections.Generic;
using NMSD.Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootStateManager
    {
        IAggregateRootState State { get; set; }
        IAggregateRootState BuildStateFromHistory(List<IEvent> events);
    }
}