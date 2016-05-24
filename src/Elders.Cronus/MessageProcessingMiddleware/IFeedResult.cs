using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.MessageProcessingMiddleware
{
    public interface IFeedResult
    {
        ISet<TransportMessage> SuccessfulMessages { get; }
        ISet<TransportMessage> FailedMessages { get; }
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

    public static class FeedResultExtentions
    {
        public static IFeedResult AppendSuccess(this IFeedResult self, TransportMessage message)
        {
            var successItems = new HashSet<TransportMessage>(self.SuccessfulMessages);
            successItems.Add(message);
            return new FeedResult(successItems, self.FailedMessages);
        }

        public static IFeedResult AppendError(this IFeedResult self, TransportMessage message, FeedError error)
        {
            var errorItems = new HashSet<TransportMessage>(self.FailedMessages);
            var errorMessage = errorItems.Where(x => x == message).SingleOrDefault() ?? message;
            errorItems.Add(new TransportMessage(errorMessage, error));

            return new FeedResult(self.SuccessfulMessages, errorItems);
        }

        public static IFeedResult AppendUnitOfWorkError(this IFeedResult self, IEnumerable<TransportMessage> messages, Exception ex)
        {
            return self.AppendError(messages, new FeedError()
            {
                Origin = new ErrorOrigin(ErrorOriginType.UnitOfWork, ErrorOriginType.UnitOfWork),
                Error = new SerializableException(ex)
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
            return new FeedResult(self.SuccessfulMessages, errorItems);
        }

        public static IFeedResult With(this IFeedResult self, IFeedResult feedResult)
        {
            var successMessages = new HashSet<TransportMessage>(self.SuccessfulMessages.Union(feedResult.SuccessfulMessages));
            var failedMessages = new HashSet<TransportMessage>(self.FailedMessages.Union(feedResult.FailedMessages));
            return new FeedResult(successMessages, failedMessages);
        }
    }
}