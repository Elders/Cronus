using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    public abstract class Publisher<TMessage> : IPublisher<TMessage> where TMessage : IMessage
    {
        static readonly ILogger logger = CronusLogger.CreateLogger(typeof(Publisher<TMessage>));
        private readonly ITenantResolver<IMessage> tenantResolver;
        private readonly BoundedContext boundedContext;

        public Publisher(ITenantResolver<IMessage> tenantResolver, BoundedContext boundedContext)
        {
            this.tenantResolver = tenantResolver;
            this.boundedContext = boundedContext;
        }

        protected abstract bool PublishInternal(CronusMessage message);

        public bool Publish(TMessage message, Dictionary<string, string> messageHeaders)
        {
            try
            {
                messageHeaders = messageHeaders ?? new Dictionary<string, string>();

                string messageId = string.Empty;
                if (messageHeaders.ContainsKey(MessageHeader.MessageId) == false)
                    messageHeaders.Add(MessageHeader.MessageId, messageId);
                else
                    messageId = messageHeaders[MessageHeader.MessageId];

                if (messageHeaders.ContainsKey(MessageHeader.CorelationId) == false)
                    messageHeaders.Add(MessageHeader.CorelationId, messageId);

                if (messageHeaders.ContainsKey(MessageHeader.PublishTimestamp) == false)
                    messageHeaders.Add(MessageHeader.PublishTimestamp, DateTime.UtcNow.ToFileTimeUtc().ToString());

                if (messageHeaders.ContainsKey(MessageHeader.Tenant) == false)
                    messageHeaders.Add(MessageHeader.Tenant, tenantResolver.Resolve(message));

                if (messageHeaders.ContainsKey(MessageHeader.BoundedContext))
                {
                    if (messageHeaders[MessageHeader.BoundedContext] == "implicit")
                        messageHeaders[MessageHeader.BoundedContext] = boundedContext.Name;
                }
                else
                {
                    var bc = message.GetType().GetBoundedContext(boundedContext.Name);
                    messageHeaders.Add(MessageHeader.BoundedContext, bc);
                }

                var cronusMessage = new CronusMessage(message, messageHeaders);
                var published = PublishInternal(cronusMessage);
                if (published == false)
                {
                    logger.Error(() => "Failed to publish => " + BuildDebugLog(message, messageHeaders));
                    return false;
                }

                logger.Info(() => message.ToString());
                logger.Debug(() => "PUBLISH => " + BuildDebugLog(message, messageHeaders));

                return true;
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => ex.Message);
                return false;
            }
        }

        public virtual bool Publish(TMessage message, DateTime publishAt, Dictionary<string, string> messageHeaders = null)
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
