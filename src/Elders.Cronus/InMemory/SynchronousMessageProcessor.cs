using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elders.Cronus.InMemory
{
    public class SynchronousMessageProcessor<T> : Publisher<IMessage> where T : IMessage
    {
        //static readonly ILog log = LogProvider.GetLogger(typeof(InMemoryPublisher<>));
        private readonly ISubscriberCollection<IApplicationService> appServiceSubscribers;
        private readonly ISubscriberCollection<ISaga> sagaSubscribers;

        //TODO
        public SynchronousMessageProcessor(ISubscriberCollection<IApplicationService> appServiceSubscribers, ISubscriberCollection<ISaga> sagaSubscribers)
            : base(new DefaultTenantResolver())
        {
            this.appServiceSubscribers = appServiceSubscribers;
            this.sagaSubscribers = sagaSubscribers;
        }

        protected override bool PublishInternal(CronusMessage message)
        {
            NotifySubscribers(message, appServiceSubscribers);
            NotifySubscribers(message, sagaSubscribers);

            return true;
        }

        void NotifySubscribers<TContract>(CronusMessage message, ISubscriberCollection<TContract> subscribers)
        {
            var interestedSubscribers = subscribers.GetInterestedSubscribers(message);
            foreach (var subscriber in interestedSubscribers)
            {
                subscriber.Process(message);
            }
        }
    }
}
