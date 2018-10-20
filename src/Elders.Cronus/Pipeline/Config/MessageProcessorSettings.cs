using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline.Config
{
    public class SubscriprionMiddlewareFactory<T> : ISubscriptionMiddleware<T>
    {
        private readonly ISubscriptionMiddleware<T> internalMiddleware;

        private readonly MessageHandlerMiddleware messageHandlerMiddleware;
        private readonly TypeContainer<T> handlerTypeContainer;

        public SubscriprionMiddlewareFactory(MessageHandlerMiddleware messageHandlerMiddleware, TypeContainer<T> handlerTypeContainer)
        {
            this.messageHandlerMiddleware = messageHandlerMiddleware;
            this.handlerTypeContainer = handlerTypeContainer;

            internalMiddleware = Create();
        }

        public ISubscriptionMiddleware<T> Create()
        {
            var subscrMiddleware = new SubscriptionMiddleware<T>();
            foreach (var type in handlerTypeContainer.Items)
            {
                var handlerSubscriber = new HandlerSubscriber(type, messageHandlerMiddleware);
                subscrMiddleware.Subscribe(handlerSubscriber);
            }

            return subscrMiddleware;
        }

        public void Subscribe(ISubscriber subscriber)
        {
            internalMiddleware.Subscribe(subscriber);
        }

        public IEnumerable<ISubscriber> GetInterestedSubscribers(CronusMessage message)
        {
            return internalMiddleware.GetInterestedSubscribers(message);
        }

        public IEnumerable<ISubscriber> Subscribers { get { return internalMiddleware.Subscribers; } }

        public void UnsubscribeAll()
        {
            internalMiddleware.UnsubscribeAll();
        }
    }
}
