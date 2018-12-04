using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;

namespace Elders.Cronus.InMemory
{
    public class InMemoryPublisher<TContract> : Publisher<TContract> where TContract : IMessage
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(InMemoryPublisher<>));

<<<<<<< Updated upstream

        public InMemoryPublisher(SubscriberCollection<object> messageProcessor)
=======
        private readonly SubscriberCollection<IApplicationService> subscribtions;
        private readonly InMemoryQueue messageQueue;

        public InMemoryPublisher(SubscriberCollection<IApplicationService> messageProcessor, InMemoryQueue messageQueue)
            : base(new DefaultTenantResolver())
>>>>>>> Stashed changes
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
