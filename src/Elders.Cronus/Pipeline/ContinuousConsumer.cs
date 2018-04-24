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

        volatile bool isRunning;

        public ContinuousConsumer(SubscriptionMiddleware subscriptions)
        {
            this.subscriptions = subscriptions;
            isRunning = true;
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
                if (isRunning)
                    WorkStart();
                else
                    WorkStop();

                while (isRunning)
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
            isRunning = false;
        }
    }
}
