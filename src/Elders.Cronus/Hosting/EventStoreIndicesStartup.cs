using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Options;

namespace Elders.Cronus
{
    [CronusStartup(Bootstraps.EventStoreIndices)]
    public class EventStoreIndicesStartup : ICronusStartup
    {
        private readonly TenantsOptions tenants;
        private readonly IPublisher<ICommand> publisher;
        private readonly TypeContainer<IEventStoreIndex> indexTypeContainer;

        public EventStoreIndicesStartup(TypeContainer<IEventStoreIndex> indexTypeContainer, IOptionsMonitor<TenantsOptions> tenantsOptions, IPublisher<ICommand> publisher)
        {
            this.tenants = tenantsOptions.CurrentValue;
            this.publisher = publisher;
            this.indexTypeContainer = indexTypeContainer;
        }

        public void Bootstrap()
        {
            foreach (var index in indexTypeContainer.Items)
            {
                foreach (var tenant in tenants.Tenants)
                {
                    var id = new EventStoreIndexManagerId(index.GetContractId(), tenant);
                    var command = new RegisterIndex(id);
                    publisher.Publish(command);
                }
            }
        }
    }

    [CronusStartup(Bootstraps.EventStoreIndices)]
    public class CronusEventStoreIndicesStartup : ICronusStartup
    {
        private readonly TenantsOptions tenants;
        private readonly IPublisher<ICommand> publisher;
        private readonly TypeContainer<ICronusEventStoreIndex> indexTypeContainer;

        public CronusEventStoreIndicesStartup(TypeContainer<ICronusEventStoreIndex> indexTypeContainer, IOptionsMonitor<TenantsOptions> tenantsOptions, IPublisher<ICommand> publisher)
        {
            this.tenants = tenantsOptions.CurrentValue;
            this.publisher = publisher;
            this.indexTypeContainer = indexTypeContainer;
        }

        public void Bootstrap()
        {
            foreach (var index in indexTypeContainer.Items)
            {
                foreach (var tenant in tenants.Tenants)
                {
                    var id = new EventStoreIndexManagerId(index.GetContractId(), tenant);
                    var command = new RegisterIndex(id);
                    publisher.Publish(command);
                }
            }
        }
    }
}
