using Elders.Cronus.EventStore.Index;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore
{
    public interface IEventStorePlayer<TSettings> : IEventStorePlayer where TSettings : class { }

    public interface IEventStorePlayer
    {
        /// <summary>
        /// Loads all aggregate commits. The commits are unordered.
        /// </summary>
        IAsyncEnumerable<AggregateCommit> LoadAggregateCommitsAsync(int batchSize = 5000);

        /// <summary>
        /// Loads all aggregate commits. The commits are unordered.
        /// </summary>
        IAsyncEnumerable<AggregateCommitRaw> LoadAggregateCommitsRawAsync(int batchSize = 5000);

        /// <summary>
        /// Loads all aggregate commits. The commits are unordered.
        /// </summary>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns></returns>
        Task<LoadAggregateCommitsResult> LoadAggregateCommitsAsync(string paginationToken, int batchSize = 5000);

        [Obsolete("Use LoadPublicEventsAsync(...)")]
        Task<LoadAggregateCommitsResult> LoadAggregateCommitsAsync(ReplayOptions replayOptions);

        /// <summary>
        /// Loads public events.
        /// </summary>
        /// <param name="replayOptions">The replay options</param>
        /// <param name="notifyProgress">If the persister supports pagination this action will be invoked when a page has been loaded and processed.</param>
        /// <param name="cancellationToken">The cancelation token.</param>
        /// <returns></returns>
        IAsyncEnumerable<IPublicEvent> LoadPublicEventsAsync(ReplayOptions replayOptions, Action<ReplayOptions> notifyProgress = null, CancellationToken cancellationToken = default);

        Task<IEvent> LoadEventWithRebuildProjectionAsync(IndexRecord indexRecord);
    }
}
