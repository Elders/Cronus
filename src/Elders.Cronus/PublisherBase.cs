using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    public abstract class PublisherBase<TMessage> : IPublisher<TMessage> where TMessage : IMessage
    {
        private readonly ITenantResolver<IMessage> tenantResolver;
        private readonly BoundedContext boundedContext;
        private readonly ILogger logger;

        public PublisherBase(ITenantResolver<IMessage> tenantResolver, BoundedContext boundedContext, ILogger logger)
        {
            this.tenantResolver = tenantResolver;
            this.boundedContext = boundedContext;
            this.logger = logger;
        }

        public virtual bool Publish(TMessage message, Dictionary<string, string> messageHeaders)
        {
            try
            {
                messageHeaders = BuildHeaders(message, messageHeaders);
                var cronusMessage = new CronusMessage(message, messageHeaders);

                using (logger.BeginScope(cronusMessage.CorelationId))
                {
                    bool isPublished = PublishInternal(cronusMessage);

                    return isPublished;
                }
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => BuildTraceData()))
            {
                return false;
            }

            string BuildTraceData()
            {
                StringBuilder errorMessage = new StringBuilder();
                errorMessage.AppendLine("Failed to publish message!");

                errorMessage.AppendLine("Headers:");
                foreach (var header in messageHeaders)
                {
                    errorMessage.AppendLine($"{header.Key}:{header.Value}");
                }

                string messageString = JsonSerializer.Serialize<object>(message);
                errorMessage.AppendLine(messageString);

                return errorMessage.ToString();
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

        protected abstract bool PublishInternal(CronusMessage message);

        protected virtual Dictionary<string, string> BuildHeaders(TMessage message, Dictionary<string, string> messageHeaders)
        {
            messageHeaders = messageHeaders ?? new Dictionary<string, string>();

            if (messageHeaders.ContainsKey(MessageHeader.PublishTimestamp) == false)
                messageHeaders.Add(MessageHeader.PublishTimestamp, DateTime.UtcNow.ToFileTimeUtc().ToString());

            if (messageHeaders.ContainsKey(MessageHeader.Tenant) == false)
                messageHeaders.Add(MessageHeader.Tenant, tenantResolver.Resolve(message));

            if (messageHeaders.ContainsKey(MessageHeader.BoundedContext))
            {
                var bc = message.GetType().GetBoundedContext(boundedContext.Name);
                messageHeaders[MessageHeader.BoundedContext] = bc;
            }
            else
            {
                var bc = message.GetType().GetBoundedContext(boundedContext.Name);
                messageHeaders.Add(MessageHeader.BoundedContext, bc);
            }

            string messageId = string.Empty;
            if (messageHeaders.ContainsKey(MessageHeader.MessageId) == false)
            {
                messageId = $"urn:cronus:{messageHeaders[MessageHeader.BoundedContext]}:{messageHeaders[MessageHeader.Tenant]}:{Guid.NewGuid()}";
                messageHeaders.Add(MessageHeader.MessageId, messageId);
            }
            else
                messageId = messageHeaders[MessageHeader.MessageId];

            if (messageHeaders.ContainsKey(MessageHeader.CorelationId) == false)
                messageHeaders.Add(MessageHeader.CorelationId, messageId);

            //if (messageHeaders.ContainsKey(MessageHeader.AggregateRootId) == false)
            //{;
            //    messageHeaders.Add(MessageHeader.AggregateRootId, message.GetType().GetContractId());
            //}

            messageHeaders.Remove("contract_name");
            messageHeaders.Add("contract_name", message.GetType().GetContractId());

            return messageHeaders;
        }
    }
}
