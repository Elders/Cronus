using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Pipeline.Hosts;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Multitenancy
{
    //public static class MultitenancyExtensions
    //{
    //    public static T WithTenants<T>(this T self, ITenantList tenants) where T : ICronusSettings
    //    {
    //        Func<ITenantList> combinedTenants = () => new ObsoleteClientTenantsIncludingElders(tenants);
    //        self.Container.RegisterSingleton(typeof(ITenantList), combinedTenants);

    //        return self;
    //    }

    //    public static T WithNoTenants<T>(this T self) where T : ICronusSettings
    //    {
    //        Func<ITenantList> combinedTenants = () => new ObsoleteClientTenantsIncludingElders();
    //        self.Container.RegisterSingleton(typeof(ITenantList), combinedTenants);

    //        return self;
    //    }
    //}

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

    public class ObsoleteClientTenantsIncludingElders : ITenantList
    {
        List<string> tenants;

        public ObsoleteClientTenantsIncludingElders(ITenantList clientTenants = null)
        {
            if (ReferenceEquals(null, clientTenants))
                tenants = new List<string>();
            else
                this.tenants = new List<string>(clientTenants.GetTenants());

            this.tenants.Add(CronusAssembly.EldersTenant);
        }

        public IEnumerable<string> GetTenants()
        {
            return tenants;
        }
    }
}
