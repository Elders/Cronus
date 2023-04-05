using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Elders.Cronus
{
    [CronusStartup(Bootstraps.Projections)]
    public class ProjectionsStartup : ICronusStartup
    {
        private readonly TenantsOptions tenants;
        private readonly ProjectionHasher hasher;
        private readonly IPublisher<ICommand> publisher;
        private readonly TypeContainer<IProjection> handlerTypeContainer;
        private readonly CronusHostOptions cronusHostOptions;

        public ProjectionsStartup(TypeContainer<IProjection> handlerTypeContainer, IOptions<CronusHostOptions> cronusHostOptions, IOptions<TenantsOptions> tenantsOptions, ProjectionHasher hasher, IPublisher<ICommand> publisher)
        {
            this.tenants = tenantsOptions.Value;
            this.hasher = hasher;
            this.publisher = publisher;
            this.handlerTypeContainer = handlerTypeContainer;
            this.cronusHostOptions = cronusHostOptions.Value;
        }

        public void Bootstrap()
        {
            if (cronusHostOptions.SystemServicesEnabled)
            {
                var systemProjection = typeof(ISystemProjection);
                foreach (var handler in handlerTypeContainer.Items.OrderByDescending(x => systemProjection.IsAssignableFrom(x)))
                {
                    foreach (var tenant in tenants.Tenants)
                    {
                        var id = new ProjectionVersionManagerId(handler.GetContractId(), tenant);
                        var command = new RegisterProjection(id, hasher.CalculateHash(handler));
                        publisher.Publish(command);
                    }
                }
            }
        }
    }
}
