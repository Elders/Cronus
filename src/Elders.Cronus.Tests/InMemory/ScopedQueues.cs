using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Elders.Cronus.InMemory
{
    public class ScopedQueues
    {
        private static ConcurrentDictionary<string, MessageQueue<CronusMessage>> queues = new ConcurrentDictionary<string, MessageQueue<CronusMessage>>();

        public IMessageQueue<CronusMessage> GetQueue(CronusMessage message)
        {
            if (queues.TryGetValue(message.CorelationId, out MessageQueue<CronusMessage> existing))
            {
                existing.Enqueue(message);
                return new AllwaysEmptyQueue<CronusMessage>();
            }
            else
            {
                var queue = new MessageQueue<CronusMessage>(message.CorelationId, Deregister);
                queues.TryAdd(message.CorelationId, queue);
                return queue;
            }
        }

        private void Deregister(MessageQueue<CronusMessage> queue)
        {
            queues.TryRemove(queue.Id, out MessageQueue<CronusMessage> removed);
        }

        public class AllwaysEmptyQueue<T> : IMessageQueue<T>
        {
            public void Enqueue(T message)
            {

            }
            public bool Any()
            {
                return false;
            }

            public T Dequeue()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
            }
        }

        public interface IMessageQueue<T> : IDisposable
        {
            void Enqueue(T message);
            bool Any();
            T Dequeue();
        }

        public class MessageQueue<T> : IMessageQueue<T>
        {
            public MessageQueue(string id, Action<MessageQueue<T>> onDispose)
            {
                this.Id = id;
                this.onDispose = onDispose;
            }

            private Queue<T> queue = new Queue<T>();

            public string Id { get; private set; }

            private readonly Action<MessageQueue<T>> onDispose;

            public void Dispose()
            {
                if (onDispose != null)
                {
                    onDispose(this);
                }
            }

            public bool Any()
            {
                return queue.Count > 0;
            }

            public T Dequeue()
            {
                return queue.Dequeue();
            }

            public void Enqueue(T message)
            {
                queue.Enqueue(message);
            }
        }
    }
}
