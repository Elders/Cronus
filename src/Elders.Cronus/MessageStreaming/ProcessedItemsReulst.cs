using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;

namespace Elders.Cronus
{

    public interface ProcessedItemsReulst<T>
    {
        IEnumerable<T> SuccessItems { get; }
        IEnumerable<TryBatch<T>> FailedBatches { get; }
    }
}