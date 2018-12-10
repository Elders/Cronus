using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Elders.Cronus.MessageProcessing
{
    public sealed class CronusContext
    {
        private string tenant;

        public string Tenant
        {
            get
            {
                if (string.IsNullOrEmpty(tenant)) throw new ArgumentException($"Unknown tenant. CronusContext is not properly built. Please call `Initialize(...)` and make sure that you properly configured `cronus_tenants`. More info at https://github.com/Elders/Cronus/blob/master/doc/Configuration.md");
                return tenant;
            }
            private set
            {
                tenant = value;
            }
        }

        public bool IsNotInitialized => string.IsNullOrEmpty(tenant) || ServiceProvider is null;

        public IServiceProvider ServiceProvider { get; private set; }

        public void Initialize(string tenant, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            EnsureValidTenant(tenant);
            Tenant = tenant;

        }

        private void EnsureValidTenant(string tenant)
        {
            if (string.IsNullOrEmpty(tenant)) throw new ArgumentNullException(nameof(tenant));

            ITenantList tenants = ServiceProvider.GetRequiredService<ITenantList>();
            if (tenants.GetTenants().Where(t => t.Equals(tenant, StringComparison.OrdinalIgnoreCase)).Any() == false)
                throw new ArgumentException($"The tenant `{tenant}` is not registered. Make sure that the tenant `{tenant}` is properly configured using `cronus_tenants`. More info at https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(tenant));
        }
    }
}
