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

        protected override IEnumerable<SubscriberMiddleware> Invoke(MiddlewareContext<Type, IEnumerable<SubscriberMiddleware>> middlewareControl)
        {
            return from subscription in subscriptions
                   where middlewareControl.Context == subscription.MessageType
                   select subscription;
        }

        public IDisposable Subscribe(SubscriberMiddleware subscriber)
        {
            if (!subscriptions.Contains(subscriber))
            {
                subscriptions.Add(subscriber);
            }
            return new Subscription(subscriptions, subscriber);
        }

        public IEnumerable<SubscriberMiddleware> GetSubscriptions()
        {
            return subscriptions.AsReadOnly();
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

        public void Dispose()
        {
            foreach (var subscription in subscriptions.ToArray())
                if (subscriptions.Contains(subscription))
                    subscription.OnCompleted();

            subscriptions.Clear();
        }
    }
}