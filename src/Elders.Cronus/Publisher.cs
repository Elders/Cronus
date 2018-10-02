﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Logging;

namespace Elders.Cronus
{
    public abstract class Publisher<TMessage> : IPublisher<TMessage> where TMessage : IMessage
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(Publisher<TMessage>));

        protected abstract bool PublishInternal(CronusMessage message);

        public bool Publish(TMessage message, Dictionary<string, string> messageHeaders)
        {
            try
            {
                messageHeaders = messageHeaders ?? new Dictionary<string, string>();
                string messageId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                messageHeaders.Add(MessageHeader.MessageId, messageId);

                if (messageHeaders.ContainsKey(MessageHeader.CorelationId) == false)
                    messageHeaders.Add(MessageHeader.CorelationId, messageId);

                if (messageHeaders.ContainsKey(MessageHeader.PublishTimestamp) == false)
                    messageHeaders.Add(MessageHeader.PublishTimestamp, DateTime.UtcNow.ToFileTimeUtc().ToString());

                var cronusMessage = new CronusMessage(message, messageHeaders);
                var published = PublishInternal(cronusMessage);
                if (published == false)
                {
                    log.Error(() => "Failed to publish => " + BuildDebugLog(message, messageHeaders));
                    return false;
                }

                log.Info(() => message.ToString());
                log.Debug(() => "PUBLISH => " + BuildDebugLog(message, messageHeaders));

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

        string BuildDebugLog(TMessage message, IDictionary<string, string> headers)
        {
            string headersInfo = string.Join(";", headers.Select(x => x.Key + "=" + x.Value).ToArray());
            return message + Environment.NewLine + headersInfo;
        }
    }
}
