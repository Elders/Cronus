using Microsoft.Extensions.Configuration;

namespace Elders.Cronus
{
    public class CronusHostOptions
    {
        private readonly IConfiguration configuration;

        public CronusHostOptions(IConfiguration configuration)
        {
            this.configuration = configuration;

            ApplicationServicesEnabled = configuration.GetValue<bool>("cronus_applicationservices_enabled", true);
            ProjectionsEnabled = configuration.GetValue<bool>("cronus_projections_enabled", true);
            PortsEnabled = configuration.GetValue<bool>("cronus_ports_enabled", true);
            SagasEnabled = configuration.GetValue<bool>("cronus_sagas_enabled", true);
            GatewaysEnabled = configuration.GetValue<bool>("cronus_gateways_enabled", true);
        }

        public bool ApplicationServicesEnabled { get; set; }
        public bool ProjectionsEnabled { get; set; }
        public bool PortsEnabled { get; set; }
        public bool SagasEnabled { get; set; }
        public bool GatewaysEnabled { get; set; }
    }
}
