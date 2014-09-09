using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventSourcing
{
    public interface IEventStream
    {
        List<IEvent> Events { get; }
        List<IAggregateRootState> Snapshots { get; }
    }
}