using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    /// <summary>
    /// A publisher with integrated logic to retry on publish failure and log additional data.
    /// </summary>
    /// <typeparam name="TMessage">The message to be sent.</typeparam>
    public abstract class Publisher<TMessage> : PublisherBase<TMessage> where TMessage : IMessage
    {
        private RetryPolicy retryPolicy;
        private readonly BoundedContext boundedContext;
        private readonly ILogger logger;

        public Publisher(ITenantResolver<IMessage> tenantResolver, BoundedContext boundedContext, ILogger logger) : base(tenantResolver, boundedContext, logger)
        {
            this.boundedContext = boundedContext;
            this.logger = logger;

            retryPolicy = new RetryPolicy(RetryableOperation.RetryPolicyFactory.CreateLinearRetryPolicy(5, TimeSpan.FromMilliseconds(300)));
        }

        public override bool Publish(TMessage message, Dictionary<string, string> messageHeaders)
        {
            try
            {
                messageHeaders = BuildHeaders(message, messageHeaders);
                var cronusMessage = new CronusMessage(message, messageHeaders);

                using (logger.BeginScope(cronusMessage.CorelationId))
                {
                    bool isPublished = RetryableOperation.TryExecute(() => PublishInternal(cronusMessage), retryPolicy, () => BuildTraceData());
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
    }
}
