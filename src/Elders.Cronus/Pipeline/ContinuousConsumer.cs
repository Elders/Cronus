using System;
using Elders.Cronus.Serializer;
using Elders.Multithreading.Scheduler;
using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline
{
    public abstract class ContinuousConsumer : IWork
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(ContinuousConsumer));

        SubscriptionMiddleware subscriptions;

        volatile bool isWorking;

        readonly ISerializer serializer;

        public ContinuousConsumer(SubscriptionMiddleware subscriptions)
        {
            this.subscriptions = subscriptions;
        }

        public string Name { get; set; }

        public DateTime ScheduledStart { get; set; }

        protected abstract void MessageConsumed(CronusMessage message);
        protected abstract void WorkStart();
        protected abstract void WorkStop();
        protected abstract CronusMessage GetMessage();

        public void Start()
        {
            try
            {
                isWorking = true;
                WorkStart();
                while (isWorking)
                {
                    CronusMessage message = GetMessage();
                    if (ReferenceEquals(null, message)) break;

                    var subscribers = subscriptions.GetInterestedSubscribers(message);
                    foreach (var subscriber in subscribers)
                    {
                        subscriber.Process(message);
                    }

                    MessageConsumed(message);
                }
            }
            catch (Exception ex)
            {
                log.ErrorException("Unexpected Exception.", ex);
            }
            finally
            {
                ScheduledStart = DateTime.UtcNow.AddMilliseconds(30);
            }
        }

        public void Stop()
        {
            isWorking = false;
            WorkStop();
        }
    }
}
