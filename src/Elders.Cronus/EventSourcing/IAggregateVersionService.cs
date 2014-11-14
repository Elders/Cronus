using Elders.Cronus.DomainModeling;
using System;

namespace Elders.Cronus.EventSourcing
{
    public interface IAggregateVersionService : IDisposable
    {
        int ReserveVersion(IAggregateRootId aggregateId, int requestedVersion);
    }
}