using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Multitenancy
{
    public class ClientTenantsIncludingElders : ITenantList
    {
        List<string> tenants;

        public ClientTenantsIncludingElders(IConfiguration configuration)
        {
            tenants = new List<string>();
            tenants.Add(CronusAssembly.EldersTenant);

            string tenantsFromConfiguration = configuration["cronus_tenants"];
            if (string.IsNullOrEmpty(tenantsFromConfiguration) == false)
            {
                var cfgTenants = configuration["cronus_tenants"].Split(',');
                foreach (var tenant in cfgTenants)
                {
                    if (tenants.Contains(tenant)) continue;
                    tenants.Add(tenant);
                }
            }
        }

        public IEnumerable<string> GetTenants()
        {
            return tenants;
        }
    }
}
