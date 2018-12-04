using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Multitenancy
{
    public class Tenants : ITenantList
    {
        public const string SettingKey = "cronus_tenants";
        public const string ValidTenantRegex = @"^\b([\w\d_]+$)";

        List<string> tenants;

        public Tenants(IConfiguration configuration)
        {
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));

            tenants = new List<string>();

            string tenantsFromConfiguration = configuration.GetRequired(SettingKey);
            if (string.IsNullOrEmpty(tenantsFromConfiguration) == false)
            {
                var cfgTenants = tenantsFromConfiguration
                    .Replace("[", "").Replace("]", "").Replace("\"", "")
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(tenant =>
                    {
                        var t = NormalizeTenant(tenant);
                        EnsureValidTenant(t);
                        return t;
                    })
                    .Distinct();



                tenants.AddRange(cfgTenants);
            }


            if (tenants.Any() == false)
                throw new ArgumentException($"{SettingKey} has no configured values. Please ensure that you have provided valid configurations for `{SettingKey}`. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(configuration));
        }

        string NormalizeTenant(string tenant)
        {
            return tenant.ToLower().Trim();
        }

        private void EnsureValidTenant(string tenant)
        {
            var regex = new Regex(ValidTenantRegex);

            if (regex.IsMatch(tenant) == false)
                throw new ArgumentException($"Invalid tenant `{tenant}`. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(tenant));

        }

        public IEnumerable<string> GetTenants() => tenants;
    }
}
