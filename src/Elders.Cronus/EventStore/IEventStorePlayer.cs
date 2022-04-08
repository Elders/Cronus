using System.Collections.Generic;
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

        Task<LoadAggregateCommitsResult> LoadAggregateCommitsAsync(ReplayOptions replayOptions);
    }
}
