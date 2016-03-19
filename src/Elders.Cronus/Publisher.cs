using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Logging;

namespace Elders.Cronus
{
    public static class MessageHeader
    {
        public const string CorelationId = "corelationid";

        public const string CausationId = "causationid";

        public const string MessageId = "messageid";

        public const string PublishTimestamp = "publictimestamp";
    }

    public abstract class Publisher<TMessage> : IPublisher<TMessage> where TMessage : IMessage
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(Publisher<TMessage>));

        protected abstract bool PublishInternal(TMessage message, Dictionary<string, string> messageHeaders);

        public bool Publish(TMessage message, Dictionary<string, string> messageHeaders)
        {
            try
            {
                messageHeaders = messageHeaders ?? new Dictionary<string, string>();
                string messageId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                messageHeaders.Add(MessageHeader.MessageId, messageId);

                PublishInternal(message, messageHeaders);
                log.Info(() => "PUBLISH => " + message);
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorException(ex.Message, ex);
                return false;
            }
        }

        public bool Publish(TMessage message, DateTime publishAt, Dictionary<string, string> messageHeaders = null)
        {
            messageHeaders = messageHeaders ?? new Dictionary<string, string>();
            messageHeaders.Add(MessageHeader.PublishTimestamp, publishAt.ToFileTimeUtc().ToString());
            return Publish(message, messageHeaders);
        }

        public bool Publish(TMessage message, TimeSpan publishAfter, Dictionary<string, string> messageHeaders = null)
        {
            DateTime publishAt = DateTime.UtcNow.Add(publishAfter);
            return Publish(message, publishAt, messageHeaders);
        }
    }
}
