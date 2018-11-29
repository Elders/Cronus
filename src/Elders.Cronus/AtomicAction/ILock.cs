using System;

namespace Elders.Cronus.AtomicAction
{
    public interface ILock
    {
        bool IsLocked(string resource);

        bool Lock(string resource, TimeSpan ttl);

        void Unlock(string resource);
    }
}
