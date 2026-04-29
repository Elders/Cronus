using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.MessageProcessing;

/// <summary>
/// Publishes live aggregate commits
/// </summary>
internal sealed class AggregateCommitPublisher : IAggregateCommitInterceptor
{
    private readonly ICronusContextAccessor contextAccessor;
    private readonly IPublisher<AggregateCommit> publisher;
    private readonly ILogger<AggregateCommitPublisher> logger;

    public AggregateCommitPublisher(IPublisher<AggregateCommit> publisher, ICronusContextAccessor contextAccessor, ILogger<AggregateCommitPublisher> logger)
    {
        this.publisher = publisher;
        this.contextAccessor = contextAccessor;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task OnAppendAsync(AggregateCommit origin, CancellationToken cancellationToken = default)
    {
        try
        {
            bool publishResult = await publisher.PublishAsync(origin, BuildHeaders(origin), cancellationToken).ConfigureAwait(false);

            if (publishResult == false)
                logger.LogError("Unable to publish aggregate commit.");
        }
        catch (Exception ex) when (True(() => logger.LogError(ex, "Unable to publish aggregate commit."))) { }
    }

    /// <inheritdoc />
    public Task<AggregateCommit> OnAppendingAsync(AggregateCommit origin, CancellationToken cancellationToken = default) => Task.FromResult(origin);

    Dictionary<string, string> BuildHeaders(AggregateCommit commit)
    {
        Dictionary<string, string> messageHeaders = new Dictionary<string, string>
        {
            { MessageHeader.AggregateRootId, Convert.ToBase64String(commit.AggregateRootId.Span) }
        };

        foreach (var trace in contextAccessor.CronusContext.Trace)
        {
            messageHeaders.Add(trace.Key, trace.Value.ToString());
        }

        return messageHeaders;
    }
}
