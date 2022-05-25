using System;
using System.Threading.Tasks;
using Elders.Cronus.Userfull;

namespace Elders.Cronus.AtomicAction
{
    public interface IAggregateRootAtomicAction : IDisposable
    {
        Task<Result<bool>> ExecuteAsync(IAggregateRootId arId, int aggregateRootRevision, Func<Task> action);
    }
}
