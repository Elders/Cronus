using System.Collections.Generic;

namespace Elders.Cronus.EventSourcing
{
    public interface IEventStorePersister
    {
        void Persist(List<DomainMessageCommit> commits);
    }
}