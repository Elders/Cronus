using System.Collections.Generic;
using Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootStateManager
    {
        IAggregateRootState State { get; set; }
        IAggregateRootState BuildFromHistory(List<IEvent> events);
    }
}