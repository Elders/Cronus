using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.MessageProcessing
{
    /// <summary>
    /// A factory for creating <see cref="HttpContext" /> instances.
    /// </summary>
    public class DefaultCronusContextFactory
    {
        private readonly ICronusContextAccessor? _cronusContextAccessor;
        private readonly ITenantResolver _tenantResolver;
        private readonly IOptionsMonitor<TenantsOptions> tenantsOptions;

        public DefaultCronusContextFactory(IServiceProvider serviceProvider)
        {
            // May be null
            _cronusContextAccessor = serviceProvider.GetService<ICronusContextAccessor>();
            _tenantResolver = serviceProvider.GetRequiredService<ITenantResolver>();
            tenantsOptions = serviceProvider.GetRequiredService<IOptionsMonitor<TenantsOptions>>();
        }

        internal ICronusContextAccessor? CronusContextAccessor => _cronusContextAccessor;

        /// <summary>
        /// Create an <see cref="HttpContext"/> instance given an <paramref name="featureCollection" />.
        /// </summary>
        /// <param name="featureCollection"></param>
        /// <returns>An initialized <see cref="HttpContext"/> object.</returns>
        public CronusContext Create(object contextTarget, IServiceProvider contextServiceProvider)
        {
            ArgumentNullException.ThrowIfNull(contextTarget);

            string tenant = _tenantResolver.Resolve(contextTarget);
            EnsureValidTenant(tenant);

            CronusContext cronusContext = new CronusContext(tenant, contextServiceProvider);

            Initialize(cronusContext);

            return cronusContext;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Initialize(CronusContext cronusContext)
        {
            Debug.Assert(cronusContext != null);

            if (_cronusContextAccessor != null)
            {
                _cronusContextAccessor.CronusContext = cronusContext;
            }
        }

        private void EnsureValidTenant(string tenant)
        {
            if (string.IsNullOrEmpty(tenant)) throw new ArgumentNullException(nameof(tenant));

            if (tenantsOptions.CurrentValue.Tenants.Where(t => t.Equals(tenant, StringComparison.OrdinalIgnoreCase)).Any() == false)
                throw new ArgumentException($"The tenant `{tenant}` is not registered. Make sure that the tenant `{tenant}` is properly configured using `cronus:tenants`. More info at https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(tenant));
        }

        /// <summary>
        /// Clears the current <see cref="HttpContext" />.
        /// </summary>
        public void Dispose(CronusContext cronusContext)
        {
            if (_cronusContextAccessor != null)
            {
                _cronusContextAccessor.CronusContext = null;
            }
        }
    }
}
