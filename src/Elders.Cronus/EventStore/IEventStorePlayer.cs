using System.Collections.Generic;

namespace Elders.Cronus.EventStore
{
    public interface IEventStorePlayer
    {
        /// <summary>
        /// Loads all aggregate commits. The commits are unordered.
        /// </summary>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns></returns>
        IEnumerable<AggregateCommit> LoadAggregateCommits(int batchSize = 100);
    }
}
