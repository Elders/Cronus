using System.Collections.Generic;

namespace Elders.Cronus
{

    public interface FeedResult<T>
    {
        IEnumerable<T> SuccessItems { get; }
        IEnumerable<TryBatch<T>> FailedBatches { get; }
    }
}