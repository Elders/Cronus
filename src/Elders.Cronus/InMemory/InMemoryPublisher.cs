using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;

namespace Elders.Cronus.InMemory
{
    public class InMemoryPublisher<TContract> : Publisher<TContract> where TContract : IMessage
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(InMemoryPublisher<>));

        SubscriberCollection<object> subscribtions;

        public InMemoryPublisher(SubscriberCollection<object> messageProcessor)
            : base(new DefaultTenantResolver())
        {
            this.subscribtions = messageProcessor;
            this.messageQueue = messageQueue;
        }

        protected override bool PublishInternal(CronusMessage message)
        {
            messageQueue.Publish(message);
            return true;
        }
    }
}
