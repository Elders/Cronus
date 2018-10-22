using Elders.Cronus.Multitenancy;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Projections.Versioning;
using System;

namespace Elders.Cronus
{
    public class ProjectionsBootstrapper
    {
        private readonly ITenantList tenants;
        private readonly TypeContainer<IProjection> projectionTypes;
        private readonly ProjectionHasher hasher;
        private readonly IPublisher<ICommand> publisher;

        public ProjectionsBootstrapper(ITenantList tenants, TypeContainer<IProjection> projectionTypes, ProjectionHasher hasher, IPublisher<ICommand> publisher)
        {
            if (tenants is null) throw new ArgumentNullException(nameof(tenants));
            if (tenants is null) throw new ArgumentNullException(nameof(projectionTypes));
            if (tenants is null) throw new ArgumentNullException(nameof(hasher));
            if (tenants is null) throw new ArgumentNullException(nameof(publisher));

            this.tenants = tenants;
            this.projectionTypes = projectionTypes;
            this.hasher = hasher;
            this.publisher = publisher;
        }

        public void Bootstrap()
        {
            foreach (var handler in projectionTypes.Items)
            {
                foreach (var tenant in tenants.GetTenants())
                {
                    var id = new ProjectionVersionManagerId($"{tenant}_{handler.GetContractId()}");
                    var command = new RegisterProjection(id, hasher.CalculateHash(handler), tenant);
                    publisher.Publish(command);
                }
            }
        }
    }
}
