using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.MessageProcessingMiddleware
{
    public interface IFeedResult
    {
        ISet<CronusMessage> SuccessfulMessages { get; }
        ISet<CronusMessage> FailedMessages { get; }
    }

    public class FeedResult : IFeedResult
    {
        public ISet<CronusMessage> SuccessfulMessages { get; private set; }
        public ISet<CronusMessage> FailedMessages { get; private set; }

        private FeedResult()
        {
            this.SuccessfulMessages = new HashSet<CronusMessage>();
            this.FailedMessages = new HashSet<CronusMessage>();
        }

        public FeedResult(IFeedResult feedResult)
            : this(feedResult.SuccessfulMessages, feedResult.FailedMessages)
        {
        }

        public FeedResult(ISet<CronusMessage> successfulMessages, ISet<CronusMessage> failedMessages)
        {
            this.SuccessfulMessages = new HashSet<CronusMessage>(successfulMessages);
            this.FailedMessages = new HashSet<CronusMessage>(failedMessages);
        }

        public static IFeedResult Empty()
        {
            return new FeedResult();
        }
    }

    public static class FeedResultExtentions
    {
        public static IFeedResult AppendSuccess(this IFeedResult self, CronusMessage message)
        {
            var successItems = new HashSet<CronusMessage>(self.SuccessfulMessages);
            successItems.Add(message);
            return new FeedResult(successItems, self.FailedMessages);
        }

        public static IFeedResult With(this IFeedResult self, IFeedResult feedResult)
        {
            var successMessages = new HashSet<CronusMessage>(self.SuccessfulMessages.Union(feedResult.SuccessfulMessages));
            var failedMessages = new HashSet<CronusMessage>(self.FailedMessages.Union(feedResult.FailedMessages));
            return new FeedResult(successMessages, failedMessages);
        }
    }
}
