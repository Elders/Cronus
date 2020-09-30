using System;
using System.Collections.Generic;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    public abstract class Publisher<TMessage> : IPublisher<TMessage> where TMessage : IMessage
    {
        static readonly ILogger logger = CronusLogger.CreateLogger(typeof(Publisher<TMessage>));

        private RetryPolicy retryPolicy;
        private readonly ITenantResolver<IMessage> tenantResolver;
        private readonly BoundedContext boundedContext;

        public Publisher(ITenantResolver<IMessage> tenantResolver, BoundedContext boundedContext)
        {
            this.tenantResolver = tenantResolver;
            this.boundedContext = boundedContext;

            retryPolicy = new RetryPolicy(RetryableOperation.RetryPolicyFactory.CreateLinearRetryPolicy(5, TimeSpan.FromMilliseconds(300)));
        }

        protected abstract bool PublishInternal(CronusMessage message);

        public bool Publish(TMessage message, Dictionary<string, string> messageHeaders)
        {
            try
            {
                messageHeaders = messageHeaders ?? new Dictionary<string, string>();

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

                messageHeaders.Remove("contract_name");
                messageHeaders.Add("contract_name", message.GetType().GetContractId());


                var cronusMessage = new CronusMessage(message, messageHeaders);

                using (logger.BeginScope(cronusMessage.CorelationId))
                {
                    bool isPublished = RetryableOperation.TryExecute(() => PublishInternal(cronusMessage), retryPolicy);
                    if (isPublished)
                    {
                        logger.Info(() => "Publish {cronus_MessageType} {cronus_MessageName} - OK", typeof(TMessage).Name, message.GetType().Name, messageHeaders);
                    }
                    else
                    {
                        logger.Error(() => "Publish {cronus_MessageType} {cronus_MessageName} - Fail", typeof(TMessage).Name, message.GetType().Name, messageHeaders);
                    }

                    return isPublished;
                }
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
    }
}
