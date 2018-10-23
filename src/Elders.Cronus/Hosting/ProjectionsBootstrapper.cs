using Elders.Cronus.Multitenancy;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Projections.Versioning;
using System;
using System.Linq;

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
            BootstrapSystemProjections();
            BootstrapProjections();
        }

        void BootstrapSystemProjections()
        {
            var projections = projectionTypes.Items.Where(t => typeof(ISystemProjection).IsAssignableFrom(t));
            foreach (var handler in projections)
            {
                var id = new ProjectionVersionManagerId($"{CronusAssembly.EldersTenant}_{handler.GetContractId()}");
                var command = new RegisterProjection(id, hasher.CalculateHash(handler), CronusAssembly.EldersTenant);
                publisher.Publish(command);
            }
        }

        void BootstrapProjections()
        {
            var projections = projectionTypes.Items.Where(t => typeof(ISystemProjection).IsAssignableFrom(t) == false);
            foreach (var handler in projections)
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
