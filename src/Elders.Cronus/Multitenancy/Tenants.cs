using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.Multitenancy
{
    public class Tenants : ITenantList
    {
        public const string SettingKey = "cronus_tenants";
        public const string ValidTenantRegex = @"^\b([\w\d_]+$)";

        static readonly object mutex = new object();
        HashSet<string> tenants;

        public Tenants(IOptionsMonitor<TenantsOptions> optionsMonitor)
        {
            if (optionsMonitor is null) throw new ArgumentNullException(nameof(optionsMonitor));

            PopulateTenants(optionsMonitor.Get(SettingKey));
            optionsMonitor.OnChange(PopulateTenants);
        }

        private void PopulateTenants(TenantsOptions options)
        {
            if (options is null || options.Tenants is null || options.Tenants.Any() == false)
                throw new ArgumentException($"{SettingKey} has no configured values. Please ensure that you have provided valid configurations for `{SettingKey}`. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(options));

            lock (mutex)
            {
                var newTenants = options.Tenants
                    .Select(x => NormalizeTenant(x))
                    .Select(x => EnsureValidTenant(x));

                tenants = new HashSet<string>(newTenants);
            }

            if (tenants.Count == 0)
                throw new ArgumentException($"{SettingKey} has no configured values. Please ensure that you have provided valid configurations for `{SettingKey}`. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(options));
        }

        string NormalizeTenant(string tenant)
        {
            return tenant.ToLower().Trim();
        }

        private string EnsureValidTenant(string tenant)
        {
            var regex = new Regex(ValidTenantRegex);

            if (regex.IsMatch(tenant) == false)
                throw new ArgumentException($"Invalid tenant `{tenant}`. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(tenant));

            return tenant;
        }

        public IEnumerable<string> GetTenants() => tenants.ToList();
    }
}
