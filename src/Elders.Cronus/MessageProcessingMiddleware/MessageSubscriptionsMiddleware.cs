using Elders.Cronus.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.MessageProcessingMiddleware
{
    public class MessageSubscriptionsMiddleware : Middleware<Type, IEnumerable<SubscriberMiddleware>>, IDisposable
    {
        readonly List<SubscriberMiddleware> subscriptions;

        public MessageSubscriptionsMiddleware()
        {
            subscriptions = new List<SubscriberMiddleware>();
        }

        public IDisposable Subscribe(SubscriberMiddleware subscriber)
        {
            if (subscriptions.Contains(subscriber) == false)
            {
                subscriptions.Add(subscriber);
            }
            return new Subscription(subscriptions, subscriber);
        }

        public IEnumerable<SubscriberMiddleware> GetSubscriptions()
        {
            return subscriptions.AsReadOnly();
        }

        public void Dispose()
        {
            foreach (var subscription in subscriptions.ToArray())
                if (subscriptions.Contains(subscription))
                    subscription.OnCompleted();

            subscriptions.Clear();
        }

        protected override IEnumerable<SubscriberMiddleware> Run(Execution<Type, IEnumerable<SubscriberMiddleware>> execution)
        {
            return from subscription in subscriptions
                   where execution.Context == subscription.MessageType
                   select subscription;
        }

        private class Subscription : IDisposable
        {
            private List<SubscriberMiddleware> subscriptions;
            private SubscriberMiddleware subscription;

            public Subscription(List<SubscriberMiddleware> subscriptions, SubscriberMiddleware subscription)
            {
                this.subscriptions = subscriptions;
                this.subscription = subscription;
            }

            public void Dispose()
            {
                if (subscription != null && subscriptions.Contains(subscription))
                    subscriptions.Remove(subscription);
            }
        }
    }
}
