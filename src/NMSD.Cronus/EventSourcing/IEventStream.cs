using System.Collections.Generic;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.EventSourcing
{
    public interface IEventStream
    {
        List<IEvent> Events { get; }
        List<IAggregateRootState> Snapshots { get; }
    }
}