using Elders.Cronus.Multitenancy;

namespace Elders.Cronus.Discoveries
{
    public class TenantsDiscovery : DiscoveryBasedOnExecutingDirAssemblies<ITenantList>
    {
        protected override DiscoveryResult<ITenantList> DiscoverFromAssemblies(DiscoveryContext context)
        {
            var result = new DiscoveryResult<ITenantList>();
            result.Models.Add(new DiscoveredModel(typeof(ITenantList), typeof(ClientTenantsIncludingElders)));
            result.Models.Add(new DiscoveredModel(typeof(ITenantResolver), typeof(DefaultTenantResolver)));

            return result;
        }
    }
}
