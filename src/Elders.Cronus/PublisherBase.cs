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
            var headers = messageHeaders is null ? new Dictionary<string, string>() : new Dictionary<string, string>(messageHeaders);

            if (headers.ContainsKey(MessageHeader.PublishTimestamp) == false)
                headers.Add(MessageHeader.PublishTimestamp, DateTime.UtcNow.ToFileTimeUtc().ToString());

            if (headers.ContainsKey(MessageHeader.Tenant) == false)
                headers.Add(MessageHeader.Tenant, tenantResolver.Resolve(message));

            if (headers.ContainsKey(MessageHeader.BoundedContext))
            {
                var bc = message.GetType().GetBoundedContext(boundedContext.Name);
                headers[MessageHeader.BoundedContext] = bc;
            }
            else
            {
                var bc = message.GetType().GetBoundedContext(boundedContext.Name);
                headers.Add(MessageHeader.BoundedContext, bc);
            }

            string messageId = string.Empty;
            if (headers.ContainsKey(MessageHeader.MessageId) == false)
            {
                messageId = $"urn:cronus:{headers[MessageHeader.BoundedContext]}:{headers[MessageHeader.Tenant]}:{Guid.NewGuid()}";
                headers.Add(MessageHeader.MessageId, messageId);
            }
            else
                messageId = headers[MessageHeader.MessageId];

            if (headers.ContainsKey(MessageHeader.CorelationId) == false)
                headers.Add(MessageHeader.CorelationId, messageId);

            headers.Remove("contract_name");
            headers.Add("contract_name", message.GetType().GetContractId());

            return headers;
        }
    }
}
