using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;
using System;
using System.Linq;
using System.Threading;

namespace Elders.Cronus.InMemory
{
    public class InMemoryConsumer<TContract> : IConsumer<TContract>
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(InMemoryConsumer<>));

        bool stopping;

        private readonly BoundedContext boundedContext;
        private readonly SubscriberCollection<TContract> subscriberCollection;
        private readonly InMemoryQueue messageQueue;

        public InMemoryConsumer(BoundedContext boundedContext, SubscriberCollection<TContract> subscriberCollection, InMemoryQueue messageQueue)
        {
            this.boundedContext = boundedContext;
            this.subscriberCollection = subscriberCollection;
            this.messageQueue = messageQueue;
        }

        public void Start()
        {
            if (subscriberCollection.Subscribers.Any() == false)
            {
                log.Warn($"Consumer {boundedContext}.{typeof(TContract).Name} not started because there are no subscribers");
                return;
            }

            try
            {
                if (stopping) return;

                while (stopping == false)
                {
                    var messageToConsume = messageQueue.Consume() as CronusMessage;
                    if (messageToConsume is null)
                        Thread.Sleep(100);
                    else
                    {
                        try
                        {
                            var subsribers = subscriberCollection.GetInterestedSubscribers(messageToConsume);

                            foreach (var subscriber in subsribers)
                            {
                                subscriber.Process(messageToConsume);
                            }
                        }
                        catch (Exception ex)
                        {
                            messageQueue.Publish(messageToConsume);
                            log.ErrorException("Failed to process message.", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorException("Unexpected Exception.", ex);
            }
        }

        public void Stop()
        {
            stopping = true;
        }
    }
}
