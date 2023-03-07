using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus;

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
        bool isPublished = RetryableOperation.TryExecute(() => base.Publish(message, messageHeaders), retryPolicy);

        return isPublished;
    }
}
