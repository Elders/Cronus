using System;
using Elders.Multithreading.Scheduler;
using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline
{
    public abstract class ContinuousConsumer : IWork
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(ContinuousConsumer));

        SubscriptionMiddleware subscriptions;

        bool stopping;

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
                if (stopping) return;

                WorkStart();
                while (stopping == false)
                {
                    CronusMessage message = GetMessage();
                    if (ReferenceEquals(null, message)) break;
                    try
                    {
                        var subscribers = subscriptions.GetInterestedSubscribers(message);
                        foreach (var subscriber in subscribers)
                        {
                            subscriber.Process(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorException("Failed to process message.", ex);
                    }
                    finally
                    {
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
                ScheduledStart = DateTime.UtcNow.AddMilliseconds(50);
            }
        }

        public void Stop()
        {
            stopping = true;
            WorkStop();
        }
    }
}
