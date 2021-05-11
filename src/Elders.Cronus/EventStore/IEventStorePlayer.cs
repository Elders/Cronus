using System.Collections.Generic;

namespace Elders.Cronus.EventStore
{
    public interface IEventStorePlayer<TSettings> : IEventStorePlayer where TSettings : class { }

    public interface IEventStorePlayer
    {
        /// <summary>
        /// Loads all aggregate commits. The commits are unordered.
        /// </summary>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns></returns>
        IEnumerable<AggregateCommit> LoadAggregateCommits(int batchSize = 5000);

        /// <summary>
        /// Loads all aggregate commits. The commits are unordered.
        /// </summary>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns></returns>
        IEnumerable<AggregateCommitRaw> LoadAggregateCommitsRaw(int batchSize = 5000);

        /// <summary>
        /// Loads all aggregate commits. The commits are unordered.
        /// </summary>
        IAsyncEnumerable<AggregateCommit> LoadAggregateCommitsAsync();

        /// <summary>
        /// Loads all aggregate commits. The commits are unordered.
        /// </summary>
        IAsyncEnumerable<AggregateCommitRaw> LoadAggregateCommitsRawAsync();

        /// <summary>
        /// Loads all aggregate commits. The commits are unordered.
        /// </summary>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns></returns>
        LoadAggregateCommitsResult LoadAggregateCommits(string paginationToken, int batchSize = 5000);

        LoadAggregateCommitsResult LoadAggregateCommits(ReplayOptions replayOptions);
    }
}
