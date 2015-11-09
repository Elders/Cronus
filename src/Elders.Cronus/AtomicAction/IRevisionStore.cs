using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Userfull;

namespace Elders.Cronus.AtomicAction
{
    /// <summary>
    /// Provides storage for aggregate revisions.
    /// </summary>
    public interface IRevisionStore : IDisposable
    {
        Result<bool> HasRevision(IAggregateRootId aggregateRootId);

        Result<int> GetRevision(IAggregateRootId aggregateRootId);

        Result<bool> SaveRevision(IAggregateRootId aggregateRootId, int revision);
    }
}
