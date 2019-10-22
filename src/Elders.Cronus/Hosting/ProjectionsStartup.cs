using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections.Versioning;
using System.Linq;

namespace Elders.Cronus
{
    [CronusStartup(Bootstraps.Projections)]
    public class ProjectionsStartup : ICronusStartup
    {
        private readonly ITenantList tenants;
        private readonly ProjectionHasher hasher;
        private readonly IPublisher<ICommand> publisher;
        private readonly TypeContainer<IProjection> handlerTypeContainer;

        public ProjectionsStartup(TypeContainer<IProjection> handlerTypeContainer, ITenantList tenants, ProjectionHasher hasher, IPublisher<ICommand> publisher)
        {
            this.tenants = tenants;
            this.hasher = hasher;
            this.publisher = publisher;
            this.handlerTypeContainer = handlerTypeContainer;
        }

        public void Bootstrap()
        {
            var systemProjection = typeof(ISystemProjection);
            foreach (var handler in handlerTypeContainer.Items.OrderByDescending(x => systemProjection.IsAssignableFrom(x)))
            {
                foreach (var tenant in tenants.GetTenants())
                {
                    var id = new ProjectionVersionManagerId(handler.GetContractId(), tenant);
                    var command = new RegisterProjection(id, hasher.CalculateHash(handler));
                    publisher.Publish(command);
                }
            }
        }
    }
}
