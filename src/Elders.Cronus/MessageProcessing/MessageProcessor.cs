using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IocContainer;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus.MessageProcessing
{
    public sealed class MessageProcessor : IObservable<MessageProcessorSubscription>, IDisposable, IMessageProcessor
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MessageProcessor));

        private readonly IContainer container;
        private readonly List<MessageProcessorSubscription> subscriptions;

        public MessageProcessor(string name, IContainer container)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            Name = name;
            subscriptions = new List<MessageProcessorSubscription>();
            this.container = container;
        }

        public string Name { get; private set; }

        public IDisposable Subscribe(MessageProcessorSubscription subscription)
        {
            if (!subscriptions.Contains(subscription))
                subscriptions.Add(subscription);
            return new Unsubscriber(subscriptions, subscription);
        }

        public IFeedResult Feed(List<TransportMessage> messages)
        {
            var feedResult = new FeedResult(PerBatchUnitOfWork(messages));
            return feedResult;
        }

        private IFeedResult PerBatchUnitOfWork(List<TransportMessage> messages)
        {
            IFeedResult feedResult = FeedResult.Empty();
            using (container.BeginScope(ScopeType.PerBatch))
            {
                var uow = container.Resolve<IUnitOfWork>();
                try
                {
                    using (uow.Begin())
                    {
                        messages.ForEach(msg =>
                        {
                            var messageFeedResult = PerMessageUnitOfWork(msg);
                            feedResult = feedResult.With(messageFeedResult);
                        });
                    }
                }
                catch (Exception ex)
                {
                    feedResult = feedResult.AppendUnitOfWorkError(uow, messages, ex);
                }
            }
            return feedResult;
        }

        private IFeedResult PerMessageUnitOfWork(TransportMessage message)
        {
            IFeedResult feedResult = FeedResult.Empty();
            using (container.BeginScope(ScopeType.PerMessage))
            {
                var uow = container.Resolve<IUnitOfWork>();
                try
                {
                    using (uow.Begin())
                    {
                        var handlerIds = from feedError in message.Errors
                                         let isUnitOfWorkError = message.Errors.Where(x => x.Origin.Type == ErrorOriginType.UnitOfWork).Any()
                                         where feedError.Origin.Type == ErrorOriginType.MessageHandler && !isUnitOfWorkError
                                         select feedError.Origin.Id.ToString();

                        var messageType = message.Payload.GetType();
                        var subscribers = from subscription in subscriptions
                                          where messageType == subscription.MessageType
                                          select subscription;

                        if (handlerIds.Count() > 0)
                            subscribers = subscribers.Where(subscription => handlerIds.Contains(subscription.Id));


                        var subscriberList = subscribers.ToList();
                        if (subscriberList.Count == 0)
                            log.WarnFormat("There is no handler for {0}", message.Payload);

                        subscriberList.ForEach(subscriber =>
                        {
                            var handlerFeedResult = PerHandlerUnitOfWork(subscriber, message);
                            feedResult = feedResult.With(handlerFeedResult);
                        });
                    }
                }
                catch (Exception ex)
                {
                    feedResult = feedResult.AppendUnitOfWorkError(uow, message, ex);
                }
            }
            return feedResult;
        }

        private IFeedResult PerHandlerUnitOfWork(MessageProcessorSubscription handlerSubscription, TransportMessage message)
        {
            var feedResult = FeedResult.Empty();
            using (container.BeginScope(ScopeType.PerHandler))
            {
                var uow = container.Resolve<IUnitOfWork>();
                try
                {
                    using (uow.Begin())
                    {
                        try
                        {
                            handlerSubscription.OnNext(message);
                            feedResult = feedResult.AppendSuccess(message);
                        }
                        catch (Exception ex)
                        {
                            handlerSubscription.OnError(ex);
                            feedResult = feedResult.AppendError(message, new FeedError()
                            {
                                Origin = new ErrorOrigin(handlerSubscription.Id, ErrorOriginType.MessageHandler),
                                Error = new Protoreg.ProtoregSerializableException(ex)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    feedResult = feedResult.AppendUnitOfWorkError(uow, message, ex);
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
        IDisposable IObservable<MessageProcessorSubscription>.Subscribe(IObserver<MessageProcessorSubscription> subscription) { throw new NotImplementedException(); }

        public IEnumerable<MessageProcessorSubscription> GetSubscriptions()
        {
            return subscriptions.AsReadOnly();
        }

        public class FeedResult : IFeedResult
        {
            public ISet<TransportMessage> SuccessfulMessages { get; private set; }
            public ISet<TransportMessage> FailedMessages { get; private set; }

            private FeedResult()
            {
                this.SuccessfulMessages = new HashSet<TransportMessage>();
                this.FailedMessages = new HashSet<TransportMessage>();
            }

            public FeedResult(IFeedResult feedResult)
                : this(feedResult.SuccessfulMessages, feedResult.FailedMessages)
            {
            }

            public FeedResult(ISet<TransportMessage> successfulMessages, ISet<TransportMessage> failedMessages)
            {
                this.SuccessfulMessages = new HashSet<TransportMessage>(successfulMessages);
                this.FailedMessages = new HashSet<TransportMessage>(failedMessages);
            }

            public static IFeedResult Empty()
            {
                return new FeedResult();
            }
        }

        private class Unsubscriber : IDisposable
        {
            private List<MessageProcessorSubscription> subscriptions;
            private MessageProcessorSubscription subscription;

            public Unsubscriber(List<MessageProcessorSubscription> subscriptions, MessageProcessorSubscription subscription)
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
