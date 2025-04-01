using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.DangerZone;

public class CronusDangerZoneOptions
{
    public CronusDangerZoneOptions()
    {
        ProtectedTenants = new List<string>();
    }

    public List<string> ProtectedTenants { get; set; }
}

public class CronusDangerZoneOptionsProvider : CronusOptionsProviderBase<CronusDangerZoneOptions>
{
    public CronusDangerZoneOptionsProvider(IConfiguration configuration) : base(configuration) { }

    public override void Configure(CronusDangerZoneOptions options)
    {
        configuration.GetSection("Cronus:DangerZone").Bind(options);
    }
}
