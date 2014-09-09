using System;

namespace Elders.Cronus.DomainModeling
{
    public interface IAggregateRootApplicationService
    {
        IAggregateRepository Repository { get; set; }
    }

    public class AggregateRootApplicationService<AR> : IAggregateRootApplicationService where AR : IAggregateRoot
    {
        public IAggregateRepository Repository { get; set; }

        public void Update(IAggregateRootId id, Action<AR> update)
        {
            var ar = Repository.Load<AR>(id);
            update(ar);
            Repository.Save(ar);
        }
    }
}