using System;
using Elders.Cronus.Serializer;
using Elders.Multithreading.Scheduler;
using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline
{
    public abstract class BaseConsumerWork : IWork
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(BaseConsumerWork));

        SubscriptionMiddleware subscriptions;

        volatile bool isWorking;

        readonly ISerializer serializer;

        public BaseConsumerWork(SubscriptionMiddleware subscriptions)
        {
            this.subscriptions = subscriptions;
        }

        public string Name { get; set; }

        public DateTime ScheduledStart { get; set; }

        protected abstract void PreConsume();
        protected abstract void ConsumeFinally();
        protected abstract void MessageConsumed(CronusMessage message);
        protected abstract void WorkStop();
        protected abstract CronusMessage GetMessage();

        public void Start()
        {
            try
            {
                isWorking = true;
                PreConsume();
                while (isWorking)
                {
                    CronusMessage message = GetMessage();
                    if (ReferenceEquals(null, message) == false)
                    {
                        var subscribers = subscriptions.GetInterestedSubscribers(message);
                        foreach (var subscriber in subscribers)
                        {
                            subscriber.Process(message);
                        }

                        MessageConsumed(message);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorException("Unexpected Exception.", ex);
            }
            finally
            {
                ScheduledStart = DateTime.UtcNow.AddMilliseconds(30);
                ConsumeFinally();
            }
        }

        public void Stop()
        {
            isWorking = false;
            WorkStop();
        }
    }
}
