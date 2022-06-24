using Microsoft.Extensions.Configuration;

namespace Elders.Cronus
{
    public class CronusHostOptions
    {
        public bool ApplicationServicesEnabled { get; set; } = true;
        public bool ProjectionsEnabled { get; set; } = true;
        public bool PortsEnabled { get; set; } = true;
        public bool SagasEnabled { get; set; } = true;
        public bool GatewaysEnabled { get; set; } = true;
        public bool TriggersEnabled { get; set; } = true;
        public bool MigrationsEnabled { get; set; } = false;
        public bool SystemServicesEnabled { get; set; } = true;
        public bool RpcApiEnabled { get; set; } = false;
    }

    public class CronusHostOptionsProvider : CronusOptionsProviderBase<CronusHostOptions>
    {
        public CronusHostOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(CronusHostOptions options)
        {
            configuration.GetSection("Cronus").Bind(options);
        }
    }
}
