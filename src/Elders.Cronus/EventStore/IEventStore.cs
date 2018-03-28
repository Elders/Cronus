using System;

namespace Elders.Cronus.EventStore
{
    public interface IEventStore
    {
        void Append(AggregateCommit aggregateCommit);
    }
}
