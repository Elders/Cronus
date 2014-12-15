using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IocContainer;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus.MessageProcessing
{
    public class MessageProcessor : IObservable<MessageProcessorSubscription>, IDisposable, IMessageProcessor
    {
        private readonly IContainer container;
        private readonly List<MessageProcessorSubscription> subscriptions;

        public MessageProcessor(IContainer container)
        {
            subscriptions = new List<MessageProcessorSubscription>();
            this.container = container;
        }

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

                        subscribers.ToList().ForEach(subscriber =>
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

        public IEnumerable<Type> GetRegisteredHandlers()
        {
            var handlerTypes = (from subscription in subscriptions
                                select subscription.MessageHandlerType)
                               .Distinct();
            return handlerTypes;
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

    public static class FeedResultExtentions
    {
        public static IFeedResult AppendSuccess(this IFeedResult self, TransportMessage message)
        {
            var successItems = new HashSet<TransportMessage>(self.SuccessfulMessages);
            successItems.Add(message);
            return new MessageProcessor.FeedResult(successItems, self.FailedMessages);
        }

        public static IFeedResult AppendError(this IFeedResult self, TransportMessage message, FeedError error)
        {
            var errorItems = new HashSet<TransportMessage>(self.FailedMessages);
            var errorMessage = errorItems.Where(x => x == message).SingleOrDefault() ?? message;
            errorItems.Add(new TransportMessage(errorMessage, error));

            return new MessageProcessor.FeedResult(self.SuccessfulMessages, errorItems);
        }

        public static IFeedResult AppendUnitOfWorkError(this IFeedResult self, IUnitOfWork uow, TransportMessage message, Exception ex)
        {
            return self.AppendError(message, new FeedError()
            {
                Origin = new ErrorOrigin(uow.Id, ErrorOriginType.UnitOfWork),
                Error = new Protoreg.ProtoregSerializableException(ex)
            });
        }

        public static IFeedResult AppendUnitOfWorkError(this IFeedResult self, IUnitOfWork uow, IEnumerable<TransportMessage> messages, Exception ex)
        {
            return self.AppendError(messages, new FeedError()
            {
                Origin = new ErrorOrigin(uow.Id, ErrorOriginType.UnitOfWork),
                Error = new Protoreg.ProtoregSerializableException(ex)
            });
        }

        public static IFeedResult AppendError(this IFeedResult self, IEnumerable<TransportMessage> messages, FeedError error)
        {
            var errorItems = new HashSet<TransportMessage>(self.FailedMessages);
            foreach (var failedMessage in messages)
            {
                var errorMessage = errorItems.Where(x => x == failedMessage).SingleOrDefault() ?? failedMessage;
                errorItems.Add(new TransportMessage(errorMessage, error));
            }
            return new MessageProcessor.FeedResult(self.SuccessfulMessages, errorItems);
        }

        public static IFeedResult With(this IFeedResult self, IFeedResult feedResult)
        {
            var successMessages = new HashSet<TransportMessage>(self.SuccessfulMessages.Union(feedResult.SuccessfulMessages));
            var failedMessages = new HashSet<TransportMessage>(self.FailedMessages.Union(feedResult.FailedMessages));
            return new MessageProcessor.FeedResult(successMessages, failedMessages);
        }
    }
}