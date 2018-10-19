using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections.Snapshotting;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Elders.Cronus.Projections
{
    public interface IProjectionStoreFactory
    {
        IProjectionStore GetProjectionStore(string tenant);
        IEnumerable<IProjectionStore> GetProjectionStores();
    }

    public interface IProjectionServicesFactory
    {
        IProjectionReader GetProjectionReader(string tenant);
        IProjectionWriter GetProjectionWriter(string tenant);

    }

    public class ProjectionServicesFactory : IProjectionServicesFactory
    {
        Dictionary<string, ProjectionRepository> factory = new Dictionary<string, ProjectionRepository>();

        public ProjectionServicesFactory(ITenantList tenants, IProjectionStoreFactory projectionStoreFactory, ISnapshotStoreFactory snapshotStoreFactory, ISnapshotStrategy snapshotStrategy, InMemoryProjectionVersionStore tovaOtpredVersionStore)
        {
            foreach (var tenant in tenants.GetTenants())
            {
                var repo = new ProjectionRepository(projectionStoreFactory.GetProjectionStore(tenant), snapshotStoreFactory.GetSnapshotStore(tenant), snapshotStrategy, tovaOtpredVersionStore);
                factory.Add(tenant, repo);
            }


        }

        public IProjectionReader GetProjectionReader(string tenant)
        {
            return factory[tenant];
        }

        public IProjectionWriter GetProjectionWriter(string tenant)
        {
            return factory[tenant];
        }
    }

}
