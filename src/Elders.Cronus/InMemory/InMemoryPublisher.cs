using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;

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

        protected override bool PublishInternal(CronusMessage message)
        {
            subscribtions.GetInterestedSubscribers(message);
            return true;
        }
    }
}
