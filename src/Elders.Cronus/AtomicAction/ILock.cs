//using System;
//using System.Collections.Concurrent;
//using System.Threading;

//namespace Elders.Cronus.AtomicAction
//{
//    public interface ILock
//    {
//        bool IsLocked(string resource);

//        bool Lock(string resource, TimeSpan ttl);

//        void Unlock(string resource);
//    }

//    public class InMemoryLockWithTTL : ILock
//    {
//        private static ConcurrentDictionary<string, Timer> locks = new ConcurrentDictionary<string, Timer>();

//        public bool IsLocked(string resource)
//        {
//            return locks.ContainsKey(resource);
//        }

//        public bool Lock(string resource, TimeSpan ttl)
//        {
//            var success = locks.TryAdd(resource, new Timer(x =>
//            {
//                Unlock(x.ToString());
//            }, resource, ttl, TimeSpan.FromMilliseconds(-1)));
//            return success;
//        }

//        public void Unlock(string resource)
//        {
//            locks.TryRemove(resource, out Timer @lock);
//        }
//    }
//}
