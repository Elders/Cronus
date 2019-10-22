using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Multitenancy;

namespace Elders.Cronus
{
    [CronusStartup(Bootstraps.EventStoreIndices)]
    public class EventStoreIndicesStartup : ICronusStartup
    {
        private readonly ITenantList tenants;
        private readonly IPublisher<ICommand> publisher;
        private readonly TypeContainer<IEventStoreIndex> indexTypeContainer;

        public EventStoreIndicesStartup(TypeContainer<IEventStoreIndex> indexTypeContainer, ITenantList tenants, IPublisher<ICommand> publisher)
        {
            this.tenants = tenants;
            this.publisher = publisher;
            this.indexTypeContainer = indexTypeContainer;
        }

        public void Bootstrap()
        {
            foreach (var index in indexTypeContainer.Items)
            {
                foreach (var tenant in tenants.GetTenants())
                {
                    var id = new EventStoreIndexManagerId(index.GetContractId(), tenant);
                    var command = new RegisterIndex(id);
                    publisher.Publish(command);
                }
            }
        }
    }
}
