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

        public CronusHost(ApplicationServicesStartup appServices, ProjectionsStartup projections, PortsStartup ports, SagasStartup sagas, GatewaysStartup gateways)
        {
            this.appServices = appServices ?? throw new ArgumentNullException(nameof(appServices));
            this.projections = projections ?? throw new ArgumentNullException(nameof(projections));
            this.ports = ports ?? throw new ArgumentNullException(nameof(ports));
            this.sagas = sagas ?? throw new ArgumentNullException(nameof(sagas));
            this.gateways = gateways ?? throw new ArgumentNullException(nameof(gateways));
        }

        public void Start()
        {
            appServices.Start();
            projections.Start();
            ports.Start();
            sagas.Start();
            gateways.Start();
        }

        public void Stop()
        {
            appServices.Stop();
            projections.Stop();
            ports.Stop();
            sagas.Stop();
            gateways.Stop();
        }

        public void Dispose() => Stop();
    }
}
