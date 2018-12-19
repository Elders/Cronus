using Elders.Cronus.Logging;
using Elders.Cronus.Multitenancy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Elders.Cronus.InMemory
{
    public abstract class InMemoryPublisher<T> : Publisher<IMessage> where T : IMessage
    {
        private readonly static ILog log = LogProvider.GetLogger(typeof(SynchronousMessageProcessor<>));

        private static ConcurrentDictionary<object, Timer> timers = new ConcurrentDictionary<object, Timer>();

        public InMemoryPublisher(ITenantResolver tenantResolver) : base(tenantResolver) { }

        public override bool Publish(IMessage message, DateTime publishAt, Dictionary<string, string> messageHeaders = null)
        {
            messageHeaders = messageHeaders ?? new Dictionary<string, string>();
            messageHeaders.Add(MessageHeader.PublishTimestamp, publishAt.ToFileTimeUtc().ToString());

            TimeSpan delta = publishAt - DateTime.UtcNow;
            if (delta.TotalSeconds > 1)
            {
                Guid timerKey = Guid.NewGuid();
                log.Debug($"{timerKey} => STARTED => {DateTime.UtcNow}");

                TimerCallback handler = (key) =>
                {
                    Publish(message, messageHeaders);
                    timers.TryRemove(key, out Timer timerInstance);
                    log.Debug($"{timerKey} => FINISHED => {DateTime.UtcNow}");
                };
                var timer = new Timer(handler, timerKey, delta, TimeSpan.FromMilliseconds(-1));
                timers.TryAdd(timerKey, timer);
            }
            else
            {
                Publish(message, messageHeaders);
            }
            return true;
        }
    }
}
