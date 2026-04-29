using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus;

/// <summary>
/// A publisher with integrated logic to retry on publish failure and log additional data.
/// </summary>
/// <typeparam name="TMessage">The message to be sent.</typeparam>
public abstract class Publisher<TMessage> : PublisherBase<TMessage> where TMessage : IMessage
{
    private readonly RetryPolicy retryPolicy;

    public Publisher(IEnumerable<DelegatingPublishHandler> handlers) : base(handlers)
    {
        retryPolicy = RetryableOperation.RetryPolicyFactory.CreateLinearRetryPolicy(5, TimeSpan.FromMilliseconds(300));
    }

    public override Task<bool> PublishAsync(TMessage message, Dictionary<string, string> messageHeaders = null, CancellationToken cancellationToken = default)
    {
        return RetryableOperation.TryExecuteAsync(
            ct => base.PublishAsync(message, messageHeaders, ct),
            retryPolicy,
            cancellationToken: cancellationToken);
    }
}
