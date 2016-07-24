using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Logging;
using Elders.Cronus.Netflix;

namespace Elders.Cronus.InMemory
{
    public class InMemoryPublisher<TContract> : Publisher<TContract> where TContract : IMessage
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(InMemoryPublisher<>));

        SubscriptionMiddleware subscribtions;

        public InMemoryPublisher(SubscriptionMiddleware messageProcessor)
        {
            this.subscribtions = messageProcessor;
        }

        protected override bool PublishInternal(TContract message, Dictionary<string, string> messageHeaders)
        {
            subscribtions.GetInterestedSubscribers(new CronusMessage(message, messageHeaders));
            return true;
        }
    }
}
