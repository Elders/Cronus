using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus
{
    public class ProjectionsBooter
    {
        private readonly ITenantList tenants;
        private readonly ProjectionHasher hasher;
        private readonly IPublisher<ICommand> publisher;
        private readonly TypeContainer<IProjection> handlerTypeContainer;

        public ProjectionsBooter(TypeContainer<IProjection> handlerTypeContainer, ITenantList tenants, ProjectionHasher hasher, IPublisher<ICommand> publisher)
        {
            this.tenants = tenants;
            this.hasher = hasher;
            this.publisher = publisher;
            this.handlerTypeContainer = handlerTypeContainer;
        }

        public void RegisterProjections()
        {
            foreach (var handler in handlerTypeContainer.Items)
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
