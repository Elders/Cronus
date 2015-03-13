using System;

namespace Elders.Cronus.DomainModeling
{
    public interface IAggregateRootAtomicAction : IDisposable
    {
        bool AtomicAction(IAggregateRootId arId, int aggregateRootRevision, Action action, out Exception error);
    }
}
