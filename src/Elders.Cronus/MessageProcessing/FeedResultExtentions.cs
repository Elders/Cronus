using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus.MessageProcessing
{
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