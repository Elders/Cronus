using System;
using System.Threading.Tasks;

namespace Elders.Cronus.AtomicAction;

public interface ILock
{
    Task<bool> IsLockedAsync(string resource);

    Task<bool> LockAsync(string resource, TimeSpan ttl);

    Task UnlockAsync(string resource);
}
