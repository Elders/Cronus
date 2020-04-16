using System;
using System.Linq;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.MessageProcessing
{
    public class CronusContextFactory
    {
        private readonly CronusContext context;
        private TenantsOptions tenants;
        private readonly ITenantResolver tenantResolver;

        public CronusContextFactory(CronusContext context, IOptionsMonitor<TenantsOptions> tenantsOptions, ITenantResolver tenantResolver)
        {
            this.context = context;
            this.tenantResolver = tenantResolver;

            this.tenants = tenantsOptions.CurrentValue;
        }

        public CronusContext GetContext(object obj, IServiceProvider serviceProvider)
        {
            if (context.IsNotInitialized)
            {
                string tenant = tenantResolver.Resolve(obj);
                EnsureValidTenant(tenant);
                context.Tenant = tenant;
                context.ServiceProvider = serviceProvider;
            }

            return context;
        }

        private void EnsureValidTenant(string tenant)
        {
            if (string.IsNullOrEmpty(tenant)) throw new ArgumentNullException(nameof(tenant));

            if (tenants.Tenants.Where(t => t.Equals(tenant, StringComparison.OrdinalIgnoreCase)).Any() == false)
                throw new ArgumentException($"The tenant `{tenant}` is not registered. Make sure that the tenant `{tenant}` is properly configured using `cronus:tenants`. More info at https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(tenant));
        }
    }
}
