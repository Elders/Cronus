using System;

namespace Elders.Cronus
{
    public sealed class CronusHost : ICronusHost
    {
        private readonly ProjectionsBooter projectionsBoot;
        private readonly IConsumer<IApplicationService> appServices;
        private readonly IConsumer<IProjection> projections;
        private readonly IConsumer<IPort> ports;
        private readonly IConsumer<ISaga> sagas;
        private readonly IConsumer<IGateway> gateways;
        private readonly CronusHostOptions cronusHostOptions;

        public CronusHost(ProjectionsBooter projectionsBoot, IConsumer<IApplicationService> appServices, IConsumer<IProjection> projections, IConsumer<IPort> ports, IConsumer<ISaga> sagas, IConsumer<IGateway> gateways, CronusHostOptions cronusHostOptions)
        {
            this.appServices = appServices ?? throw new ArgumentNullException(nameof(appServices));
            this.projections = projections ?? throw new ArgumentNullException(nameof(projections));
            this.ports = ports ?? throw new ArgumentNullException(nameof(ports));
            this.sagas = sagas ?? throw new ArgumentNullException(nameof(sagas));
            this.gateways = gateways ?? throw new ArgumentNullException(nameof(gateways));

            this.projectionsBoot = projectionsBoot;
            this.cronusHostOptions = cronusHostOptions;
        }

        public void Start()
        {
            if (cronusHostOptions.ApplicationServicesEnabled) appServices.Start();
            if (cronusHostOptions.SagasEnabled) sagas.Start();
            if (cronusHostOptions.ProjectionsEnabled)
            {
                projections.Start();
                //projectionsBoot.Bootstrap();
            }
            if (cronusHostOptions.PortsEnabled) ports.Start();

            if (cronusHostOptions.GatewaysEnabled) gateways.Start();
        }

        public void Stop()
        {
            if (cronusHostOptions.ApplicationServicesEnabled) appServices.Stop();
            if (cronusHostOptions.SagasEnabled) sagas.Stop();
            if (cronusHostOptions.ProjectionsEnabled) projections.Stop();
            if (cronusHostOptions.PortsEnabled) ports.Stop();

            if (cronusHostOptions.GatewaysEnabled) gateways.Stop();
        }

        public void Dispose() => Stop();
    }
}
