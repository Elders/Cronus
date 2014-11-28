using System;

namespace Elders.Cronus.DomainModeling
{
    public interface IAggregateRevisionService : IDisposable
    {
        int ReserveRevision(IAggregateRootId aggregateId, int requestedRevision);
    }
}