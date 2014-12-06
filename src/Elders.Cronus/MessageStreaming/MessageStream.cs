using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;

namespace Elders.Cronus
{
    public class MessageStream : IObservable<Subscription>, IDisposable, IMessageProcessor<IMessage>
    {
        private readonly IContainer container;
        private readonly List<Subscription> subscriptions;

        public MessageStream(IContainer container)
        {
            subscriptions = new List<Subscription>();
            this.container = container;
        }

        public IDisposable Subscribe(Subscription subscription)
        {
            if (!subscriptions.Contains(subscription))
                subscriptions.Add(subscription);
            return new Unsubscriber(subscriptions, subscription);
        }

        public ISafeBatchResult<TransportMessage> Feed(List<TransportMessage> messages)
        {
            var feedResult = new FeedReslt();
            using (container.BeginScope(ScopeType.PerBatch))
            {
                foreach (var transportMessage in messages)
                {
                    var messageType = transportMessage.Payload.GetType();
                    using (container.BeginScope(ScopeType.PerMessage))
                    {
                        var subscribers = from subscription in subscriptions
                                          where messageType == subscription.MessageType
                                          select subscription;

                        subscribers.ToList().ForEach(x =>
                        {
                            using (container.BeginScope(ScopeType.PerHandler))
                            {
                                try
                                {
                                    x.OnNext(transportMessage);
                                    feedResult.SuccessItems.ToList().Add(transportMessage);
                                }
                                catch (Exception ex)
                                {
                                    x.OnError(ex);
                                    feedResult.FailedBatches.ToList().Add(new TryBatch<TransportMessage>(new List<TransportMessage>() { transportMessage }, ex));
                                }
                            }
                        });
                    }
                }
            }
            return feedResult;
        }

        public void Dispose()
        {
            foreach (var subscription in subscriptions.ToArray())
                if (subscriptions.Contains(subscription))
                    subscription.OnCompleted();

            subscriptions.Clear();
        }

        //  Tova e taka narochno.
        IDisposable IObservable<Subscription>.Subscribe(IObserver<Subscription> subscription) { throw new NotImplementedException(); }

        public ISafeBatchResult<TransportMessage> Handle(List<TransportMessage> messages)
        {
            return Feed(messages);
        }

        public IEnumerable<Type> GetRegisteredHandlers()
        {
            var handlerTypes = (from subscription in subscriptions
                                select subscription.MessageHandlerType)
                               .Distinct();
            return handlerTypes;
        }

        private class FeedReslt : ISafeBatchResult<TransportMessage>
        {
            public FeedReslt()
            {
                SuccessItems = new List<TransportMessage>();
                FailedBatches = new List<TryBatch<TransportMessage>>();
            }

            public IEnumerable<TryBatch<TransportMessage>> FailedBatches { get; set; }

            public IEnumerable<TransportMessage> SuccessItems { get; set; }
        }

        private class Unsubscriber : IDisposable
        {
            private List<Subscription> subscriptions;
            private Subscription subscription;

            public Unsubscriber(List<Subscription> subscriptions, Subscription subscription)
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