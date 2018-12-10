using Elders.Cronus.MessageProcessing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Elders.Cronus.InMemory
{
    public class InMemoryQueue
    {
        private readonly ConcurrentQueue<CronusMessage> messageQueue;

        public InMemoryQueue()
        {
            messageQueue = new ConcurrentQueue<CronusMessage>();
        }

        public void Publish(CronusMessage message)
        {
            messageQueue.Enqueue(message);
        }

        public CronusMessage Consume()
        {
            messageQueue.TryDequeue(out CronusMessage message);
            return message;
        }
    }

    public class SingletonPerTenantContainer<T> : IDisposable
    {
        public SingletonPerTenantContainer()
        {
            Stash = new ConcurrentDictionary<string, T>();
        }

        public ConcurrentDictionary<string, T> Stash { get; private set; }

        public void Dispose()
        {
            foreach (var item in Stash.Values)
            {
                if (item is IDisposable)
                    (item as IDisposable).Dispose();
            }
            Stash.Clear();
        }
    }
}
