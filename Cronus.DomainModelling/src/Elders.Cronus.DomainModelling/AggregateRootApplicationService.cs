using System;

namespace Elders.Cronus.DomainModelling
{
    public interface IAggregateRootApplicationService<AR> where AR : IAggregateRoot
    {
        IAggregateRepository<AR> Repository { get; set; }
    }

    public class AggregateRootApplicationService<AR> : IAggregateRootApplicationService<AR> where AR : IAggregateRoot
    {
        public IAggregateRepository<AR> Repository { get; set; }

        public void Update(IAggregateRootId id, Action<AR> update)
        {
            var ar = Repository.Load<AR>(id);
            update(ar);
            Repository.Save(ar);
        }
    }
}