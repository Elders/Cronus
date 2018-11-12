using System;

namespace Elders.Cronus
{
    public sealed class CronusHost : ICronusHost
    {
        private readonly ApplicationServicesStartup appServices;
        private readonly ProjectionsStartup projections;
        private readonly PortsStartup ports;
        private readonly SagasStartup sagas;
        private readonly GatewaysStartup gateways;
        private readonly CronusHostOptions cronusHostOptions;

        public CronusHost(ApplicationServicesStartup appServices, ProjectionsStartup projections, PortsStartup ports, SagasStartup sagas, GatewaysStartup gateways, CronusHostOptions cronusHostOptions)
        {
            this.appServices = appServices ?? throw new ArgumentNullException(nameof(appServices));
            this.projections = projections ?? throw new ArgumentNullException(nameof(projections));
            this.ports = ports ?? throw new ArgumentNullException(nameof(ports));
            this.sagas = sagas ?? throw new ArgumentNullException(nameof(sagas));
            this.gateways = gateways ?? throw new ArgumentNullException(nameof(gateways));
            this.cronusHostOptions = cronusHostOptions;
        }

        public void Start()
        {
            if (cronusHostOptions.ApplicationServicesEnabled) appServices.Start();
            if (cronusHostOptions.ProjectionsEnabled) projections.Start();
            if (cronusHostOptions.PortsEnabled) ports.Start();
            if (cronusHostOptions.SagasEnabled) sagas.Start();
            if (cronusHostOptions.GatewaysEnabled) gateways.Start();
        }

        public void Stop()
        {
            if (cronusHostOptions.ApplicationServicesEnabled) appServices.Stop();
            if (cronusHostOptions.ProjectionsEnabled) projections.Stop();
            if (cronusHostOptions.PortsEnabled) ports.Stop();
            if (cronusHostOptions.SagasEnabled) sagas.Stop();
            if (cronusHostOptions.GatewaysEnabled) gateways.Stop();
        }

        public void Dispose() => Stop();
    }
}
