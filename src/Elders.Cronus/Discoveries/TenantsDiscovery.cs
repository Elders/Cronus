using System.Collections.Generic;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class TenantsDiscovery : DiscoveryBase<ITenantList>
    {
        protected override DiscoveryResult<ITenantList> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<ITenantList>(GetModels());
        }

        IEnumerable<DiscoveredModel> GetModels()
        {
            yield return new DiscoveredModel(typeof(ITenantList), typeof(Tenants), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ITenantResolver), typeof(DefaultTenantResolver), ServiceLifetime.Singleton);
        }
    }
}
