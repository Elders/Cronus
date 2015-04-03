using System;
using System.Collections.Generic;

namespace Elders.Cronus.EventStore
{
    public interface IEventStorePlayer
    {
        IEnumerable<AggregateCommit> GetFromStart(int batchPerQuery = 1);
        IEnumerable<AggregateCommit> GetFromStart(DateTime start, DateTime end, int batchPerQuery = 1);
    }
}
