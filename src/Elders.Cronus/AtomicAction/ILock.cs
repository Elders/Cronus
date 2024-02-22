using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.AtomicAction;

public interface ILock
{
    Task<bool> IsLockedAsync(string resource);

    Task<bool> LockAsync(string resource, TimeSpan ttl);

    Task UnlockAsync(string resource);
}

public class InMemoryLockWithTTL : ILock
{
    private static ConcurrentDictionary<string, Timer> locks = new ConcurrentDictionary<string, Timer>();

    public Task<bool> IsLockedAsync(string resource)
    {
        return Task.FromResult(locks.ContainsKey(resource));
    }

    public Task<bool> LockAsync(string resource, TimeSpan ttl)
    {
        var success = locks.TryAdd(resource, new Timer(x =>
        {
            UnlockAsync(x.ToString());
        }, resource, ttl, TimeSpan.FromMilliseconds(-1)));
        return Task.FromResult(success);
    }

    public Task UnlockAsync(string resource)
    {
        locks.TryRemove(resource, out Timer @lock);
        return Task.CompletedTask;
    }
}
