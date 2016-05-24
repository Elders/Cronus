using Elders.Cronus.Logging;
using Elders.Cronus.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.MessageProcessingMiddleware
{
    public class TransportMessageProcessorMiddleware : Middleware<TransportMessage, IFeedResult>
    {
        Middleware<Type, IEnumerable<SubscriberMiddleware>> messageSubscriptionsMiddleware;

        public TransportMessageProcessorMiddleware(Middleware<Type, IEnumerable<SubscriberMiddleware>> messageSubscriptionsMiddleware)
        {
            this.messageSubscriptionsMiddleware = messageSubscriptionsMiddleware;
        }

        static readonly ILog log = LogProvider.GetLogger(typeof(TransportMessageProcessorMiddleware));

        protected override IFeedResult Invoke(TransportMessage message, MiddlewareExecution<TransportMessage, IFeedResult> middlewareControl)
        {
            IFeedResult feedResult = FeedResult.Empty();
            try
            {
                var handlerIds = from feedError in message.Errors
                                 let isUnitOfWorkError = message.Errors.Any(x => x.Origin.Type == ErrorOriginType.UnitOfWork)
                                 where feedError.Origin.Type == ErrorOriginType.MessageHandler && !isUnitOfWorkError
                                 select feedError.Origin.Id.ToString();

                var messageType = message.Payload.Payload.GetType();

                var subscribers = messageSubscriptionsMiddleware.Invoke(messageType);


                if (handlerIds.Count() > 0)
                    subscribers = subscribers.Where(subscription => handlerIds.Contains(subscription.Id));

                var subscriberList = subscribers.ToList();
                if (subscriberList.Count == 0)
                    log.WarnFormat("There is no handler/subscriber for {0}", message.Payload);

                subscriberList.ForEach(subscriber =>
                {
                    var handlerFeedResult = PerHandlerUnitOfWork(subscriber, message);
                    feedResult = feedResult.With(handlerFeedResult);
                });
            }
            catch (Exception ex)
            {
                feedResult = feedResult.AppendUnitOfWorkError(new List<TransportMessage>() { message }, ex);
            }
            return feedResult;
        }

        private IFeedResult PerHandlerUnitOfWork(SubscriberMiddleware subscriber, TransportMessage message)
        {
            var feedResult = FeedResult.Empty();
            try
            {
                subscriber.Invoke(message.Payload);
                feedResult = feedResult.AppendSuccess(message);
            }
            catch (Exception ex)
            {
                feedResult = feedResult.AppendError(message, new FeedError()
                {
                    Origin = new ErrorOrigin(subscriber.Id, ErrorOriginType.MessageHandler),
                    Error = new SerializableException(ex)
                });
            }

            return feedResult;
        }
    }
}