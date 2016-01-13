using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Userfull;

namespace Elders.Cronus.AtomicAction
{
    public interface IAggregateRootAtomicAction : IDisposable
    {
        Result<bool> Execute(IAggregateRootId arId, int aggregateRootRevision, Action action);
    }
}
