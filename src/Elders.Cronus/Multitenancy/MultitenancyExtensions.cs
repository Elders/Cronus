using System;
using System.Collections.Generic;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Multitenancy
{
    public static class MultitenancyExtensions
    {
        public static T WithTenants<T>(this T self, ITenantList tenants) where T : ICronusSettings
        {
            Func<ITenantList> combinedTenants = () => new ClientTenantsIncludingElders(tenants);
            self.Container.RegisterSingleton(typeof(ITenantList), combinedTenants);

            return self;
        }

        public static T WithNoTenants<T>(this T self) where T : ICronusSettings
        {
            Func<ITenantList> combinedTenants = () => new ClientTenantsIncludingElders();
            self.Container.RegisterSingleton(typeof(ITenantList), combinedTenants);

            return self;
        }
    }

    public class ClientTenantsIncludingElders : ITenantList
    {
        List<string> tenants;

        public ClientTenantsIncludingElders(ITenantList clientTenants = null)
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
