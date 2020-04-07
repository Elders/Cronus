using Elders.Cronus.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Elders.Cronus.Multitenancy
{
    public class TenantsOptions
    {
        public IEnumerable<string> Tenants { get; set; }
    }

    public class TenantsOptionsProvider : CronusOptionsProviderBase<TenantsOptions>
    {
        public const string SettingKey = "cronus:tenants";
        public const string ValidTenantRegex = @"^\b([\w\d_]+$)";
        Regex regex;

        public TenantsOptionsProvider(IConfiguration configuration) : base(configuration)
        {
            regex = new Regex(ValidTenantRegex);
        }

        public override void Configure(TenantsOptions options)
        {
            options.Tenants = configuration.GetSection(SettingKey).Get<string[]>();
            PopulateTenants(options);
        }

        public override void PostConfigure(string name, TenantsOptions options)
        {
            PopulateTenants(options);
        }

        private void PopulateTenants(TenantsOptions options)
        {
            if (options is null || options.Tenants is null || options.Tenants.Any() == false)
                throw new ArgumentException($"{SettingKey} has no configured values. Please ensure that you have provided valid configurations for `{SettingKey}`. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(options));

            var newTenants = options.Tenants
                .Select(x => NormalizeTenant(x))
                .Select(x => EnsureValidTenant(x));

            options.Tenants = new HashSet<string>(newTenants);


            if (options.Tenants.Any() == false)
                throw new ArgumentException($"{SettingKey} has no configured values. Please ensure that you have provided valid configurations for `{SettingKey}`. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(options));
        }

        string NormalizeTenant(string tenant)
        {
            return tenant.ToLower().Trim();
        }

        private string EnsureValidTenant(string tenant)
        {
            if (regex.IsMatch(tenant) == false)
                throw new ArgumentException($"Invalid tenant `{tenant}`. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md", nameof(tenant));

            return tenant;
        }
    }
}
