using System.Collections.Generic;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.EventSourcing
{
    public interface IEventStorePersister
    {
        void Persist(List<IDomainMessageCommit> commits);
    }
}