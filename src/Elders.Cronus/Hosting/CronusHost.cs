using System;

namespace Elders.Cronus
{
    public sealed class CronusHost : ICronusHost
    {
        private readonly IConsumer<IAggregateRootApplicationService> appServices;
        private readonly IConsumer<IProjection> projections;
        private readonly IConsumer<IPort> ports;
        private readonly IConsumer<ISaga> sagas;
        private readonly IConsumer<IGateway> gateways;
        private readonly ProjectionsBootstrapper projectionsBootstrapper;

        public CronusHost(IConsumer<IAggregateRootApplicationService> appServices, IConsumer<IProjection> projections, IConsumer<IPort> ports, IConsumer<ISaga> sagas, IConsumer<IGateway> gateways, ProjectionsBootstrapper projectionsBootstrapper)
        {
            this.appServices = appServices ?? throw new ArgumentNullException(nameof(appServices));
            this.projections = projections ?? throw new ArgumentNullException(nameof(projections));
            this.ports = ports ?? throw new ArgumentNullException(nameof(ports));
            this.sagas = sagas ?? throw new ArgumentNullException(nameof(sagas));
            this.gateways = gateways ?? throw new ArgumentNullException(nameof(gateways));
            this.projectionsBootstrapper = projectionsBootstrapper ?? throw new ArgumentNullException(nameof(projectionsBootstrapper));
        }

        public void Start()
        {
            appServices.Start();
            projections.Start();
            ports.Start();
            sagas.Start();
            gateways.Start();

            projectionsBootstrapper.Bootstrap();
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
