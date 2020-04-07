using Microsoft.Extensions.Options;
using System;

namespace Elders.Cronus
{
    public sealed class CronusHost : ICronusHost
    {
        private readonly BoundedContext vc;
        private readonly IConsumer<IApplicationService> appServices;
        private readonly IConsumer<IProjection> projections;
        private readonly IConsumer<IPort> ports;
        private readonly IConsumer<ISaga> sagas;
        private readonly IConsumer<IGateway> gateways;
        private CronusHostOptions hostOptions;

        public CronusHost(IOptionsMonitor<BoundedContext> vc, IConsumer<IApplicationService> appServices, IConsumer<IProjection> projections, IConsumer<IPort> ports, IConsumer<ISaga> sagas, IConsumer<IGateway> gateways, IOptionsMonitor<CronusHostOptions> cronusHostOptions)
        {
            this.vc = vc.CurrentValue;
            this.appServices = appServices ?? throw new ArgumentNullException(nameof(appServices));
            this.projections = projections ?? throw new ArgumentNullException(nameof(projections));
            this.ports = ports ?? throw new ArgumentNullException(nameof(ports));
            this.sagas = sagas ?? throw new ArgumentNullException(nameof(sagas));
            this.gateways = gateways ?? throw new ArgumentNullException(nameof(gateways));

            this.hostOptions = cronusHostOptions.CurrentValue;
            cronusHostOptions.OnChange(Changed);
        }

        public void Start()
        {
            if (hostOptions.ApplicationServicesEnabled) appServices.Start();
            if (hostOptions.SagasEnabled) sagas.Start();
            if (hostOptions.ProjectionsEnabled) projections.Start();
            if (hostOptions.PortsEnabled) ports.Start();
            if (hostOptions.GatewaysEnabled) gateways.Start();
        }

        public void Stop()
        {
            if (hostOptions.ApplicationServicesEnabled) appServices.Stop();
            if (hostOptions.SagasEnabled) sagas.Stop();
            if (hostOptions.ProjectionsEnabled) projections.Stop();
            if (hostOptions.PortsEnabled) ports.Stop();
            if (hostOptions.GatewaysEnabled) gateways.Stop();
        }

        public void Dispose() => Stop();

        private void Changed(CronusHostOptions newOptions)
        {
            if (hostOptions != newOptions)
            {
                Start(hostOptions, newOptions);
                Stop(hostOptions, newOptions);

                hostOptions = newOptions;
            }
        }

        private void Start(CronusHostOptions oldOptions, CronusHostOptions newOptions)
        {
            if (oldOptions.ApplicationServicesEnabled == false && newOptions.ApplicationServicesEnabled == true) appServices.Start();
            if (oldOptions.SagasEnabled == false && newOptions.SagasEnabled == true) sagas.Start();
            if (oldOptions.ProjectionsEnabled == false && newOptions.ProjectionsEnabled == true) projections.Start();
            if (oldOptions.PortsEnabled == false && newOptions.PortsEnabled == true) ports.Start();
            if (oldOptions.GatewaysEnabled == false && newOptions.GatewaysEnabled == true) gateways.Start();
        }

        private void Stop(CronusHostOptions oldOptions, CronusHostOptions newOptions)
        {
            if (oldOptions.ApplicationServicesEnabled == true && newOptions.ApplicationServicesEnabled == false) appServices.Stop();
            if (oldOptions.SagasEnabled == true && newOptions.SagasEnabled == false) sagas.Stop();
            if (oldOptions.ProjectionsEnabled == true && newOptions.ProjectionsEnabled == false) projections.Stop();
            if (oldOptions.PortsEnabled == true && newOptions.PortsEnabled == false) ports.Stop();
            if (oldOptions.GatewaysEnabled == true && newOptions.GatewaysEnabled == false) gateways.Stop();
        }
    }
}

