using System;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.AtomicAction
{
    public interface IAggregateRootLock : IDisposable
    {
        bool IsLocked(IAggregateRootId aggregateRootId);

        object Lock(IAggregateRootId aggregateRootId, TimeSpan ttl);

        void Unlock(object mutex);
    }
}