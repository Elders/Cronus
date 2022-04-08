//using Elders.Cronus.EventStore.Index;
//using Elders.Cronus.MessageProcessing;
//using Elders.Cronus.Multitenancy;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System;
//using System.Threading.Tasks;

//namespace Elders.Cronus.InMemory
//{
//    public class InMemoryPublisher<T> : Publisher<IMessage> where T : IMessage
//    {
//        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(InMemoryPublisher<>));

//        private readonly ISubscriberCollection<IApplicationService> appServiceSubscribers;
//        private readonly ISubscriberCollection<IProjection> projectionSubscribers;
//        private readonly ISubscriberCollection<IPort> portSubscribers;
//        private readonly ISubscriberCollection<IGateway> gatewaySubscribers;
//        private readonly ISubscriberCollection<ISaga> sagaSubscribers;
//        private readonly ISubscriberCollection<IEventStoreIndex> esIndexSubscribers;
//        private readonly ScopedQueues queues = new ScopedQueues();

//        public InMemoryPublisher(
//            ISubscriberCollection<IApplicationService> appServiceSubscribers,
//            ISubscriberCollection<IProjection> projectionSubscribers,
//            ISubscriberCollection<IPort> portSubscribers,
//            ISubscriberCollection<IGateway> gatewaySubscribers,
//            ISubscriberCollection<ISaga> sagaSubscribers,
//            ISubscriberCollection<IEventStoreIndex> esIndexSubscribers,
//            IOptionsMonitor<BoundedContext> boundedContext)
//            : base(new DefaultTenantResolver(), boundedContext.CurrentValue, logger)
//        {
//            this.appServiceSubscribers = appServiceSubscribers;
//            this.projectionSubscribers = projectionSubscribers;
//            this.portSubscribers = portSubscribers;
//            this.gatewaySubscribers = gatewaySubscribers;
//            this.sagaSubscribers = sagaSubscribers;
//            this.esIndexSubscribers = esIndexSubscribers;
//        }

//        protected override bool PublishInternal(CronusMessage message)
//        {
//            throw new NotImplementedException();
//        }

//        protected override async Task<bool> PublishInternal(CronusMessage message)
//        {
//            using (var queue = queues.GetQueue(message))
//            {
//                queue.Enqueue(message);
//                while (queue.Any())
//                {
//                    var msg = queue.Dequeue();
//                    await Task.WhenAll(
//                            NotifySubscribersAsync(msg, appServiceSubscribers),
//                            NotifySubscribersAsync(msg, esIndexSubscribers),
//                            NotifySubscribersAsync(msg, projectionSubscribers),
//                            NotifySubscribersAsync(msg, portSubscribers),
//                            NotifySubscribersAsync(msg, gatewaySubscribers),
//                            NotifySubscribersAsync(msg, sagaSubscribers)
//                        );

//                }
//            }

//            return true;
//        }

//        async Task NotifySubscribersAsync<TContract>(CronusMessage message, ISubscriberCollection<TContract> subscribers)
//        {
//            try
//            {
//                var interestedSubscribers = subscribers.GetInterestedSubscribers(message);
//                foreach (var subscriber in interestedSubscribers)
//                {
//                    await subscriber.ProcessAsync(message).ConfigureAwait(false);
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.ErrorException(ex, () => "Unable to process message");
//            }
//        }
//    }
//}
