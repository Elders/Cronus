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


    public class TenantsWithoutElders : ITenantList
    {
        List<string> tenants;

        public TenantsWithoutElders(IConfiguration configuration)
        {
            tenants = new List<string>();

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

        public TenantsWithoutElders(IEnumerable<string> tenants)
        {
            tenants = tenants.ToList();
        }

        public IEnumerable<string> GetTenants()
        {
            return tenants;
        }
    }


    public static class MultitenancyExtensions
    {
        public static bool HasOtherTenantThanElders(this ITenantList tenants)
        {
            return tenants.GetTenants().Count() > 1 ||
                (
                    tenants.GetTenants().Count() == 1 &&
                    tenants.GetTenants().Any(t => t.Equals(CronusAssembly.EldersTenant) == false
                ));
        }

        public static ITenantList GetTenantsWithoutElders(this ITenantList tenants)
        {
            return new TenantsWithoutElders(tenants.GetTenants().Where(x => x != CronusAssembly.EldersTenant));
        }
    }

}
