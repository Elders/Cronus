using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore
{
    public interface IEventStorePersister
    {
        void Persist(AggregateCommit aggregateCommit);
        List<AggregateCommit> Load(IAggregateRootId aggregateId);
    }
}