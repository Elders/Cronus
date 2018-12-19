using Elders.Cronus.Logging;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Multitenancy;
using System;

namespace Elders.Cronus.InMemory
{
    public class SynchronousMessageProcessor<T> : InMemoryPublisher<IMessage> where T : IMessage
    {
        private readonly static ILog log = LogProvider.GetLogger(typeof(SynchronousMessageProcessor<>));

        private readonly ISubscriberCollection<IApplicationService> appServiceSubscribers;
        private readonly ISubscriberCollection<IProjection> projectionSubscribers;
        private readonly ISubscriberCollection<IPort> portSubscribers;
        private readonly ISubscriberCollection<IGateway> gatewaySubscribers;
        private readonly ISubscriberCollection<ISaga> sagaSubscribers;

        public SynchronousMessageProcessor(
            ISubscriberCollection<IApplicationService> appServiceSubscribers,
            ISubscriberCollection<IProjection> projectionSubscribers,
            ISubscriberCollection<IPort> portSubscribers,
            ISubscriberCollection<IGateway> gatewaySubscribers,
            ISubscriberCollection<ISaga> sagaSubscribers)
            : base(new DefaultTenantResolver())
        {
            this.appServiceSubscribers = appServiceSubscribers;
            this.projectionSubscribers = projectionSubscribers;
            this.portSubscribers = portSubscribers;
            this.gatewaySubscribers = gatewaySubscribers;
            this.sagaSubscribers = sagaSubscribers;
        }

        protected override bool PublishInternal(CronusMessage message)
        {
            NotifySubscribers(message, appServiceSubscribers);
            NotifySubscribers(message, projectionSubscribers);
            NotifySubscribers(message, portSubscribers);
            NotifySubscribers(message, gatewaySubscribers);
            NotifySubscribers(message, sagaSubscribers);

            return true;
        }

        void NotifySubscribers<TContract>(CronusMessage message, ISubscriberCollection<TContract> subscribers)
        {
            try
            {
                var interestedSubscribers = subscribers.GetInterestedSubscribers(message);
                foreach (var subscriber in interestedSubscribers)
                {
                    subscriber.Process(message);
                }
            }
            catch (Exception ex)
            {
                log.Error("Unable to process message", ex);
            }
        }
    }
}
