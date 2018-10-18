using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Multitenancy
{
    public class ClientTenantsIncludingElders : ITenantList
    {
        List<string> tenants;

        public ClientTenantsIncludingElders(IConfiguration configuration)
        {
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));

            tenants = new List<string>();
            tenants.Add(CronusAssembly.EldersTenant);

            string tenantsFromConfiguration = configuration["cronus_tenants"];
            if (string.IsNullOrEmpty(tenantsFromConfiguration) == false)
            {
                var cfgTenants = configuration["cronus_tenants"]
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(tenant => tenant.ToLower())
                    .Distinct();

                tenants.AddRange(cfgTenants);
            }
        }

        public IEnumerable<string> GetTenants() => tenants;
    }
}
