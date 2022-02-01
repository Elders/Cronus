using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Elders.Cronus
{
    public sealed class CronusHost : ICronusHost
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(CronusHost));
        private readonly CronusBooter booter;
        private readonly IConsumer<IApplicationService> appServices;
        private readonly IConsumer<ICronusEventStoreIndex> systemIndices;
        private readonly IConsumer<IEventStoreIndex> indices;
        private readonly IConsumer<IProjection> projections;
        private readonly IConsumer<IPort> ports;
        private readonly IConsumer<ISaga> sagas;
        private readonly IConsumer<IGateway> gateways;
        private readonly IConsumer<ITrigger> triggers;
        private readonly IConsumer<ISystemAppService> systemAppServices;
        private readonly IConsumer<ISystemSaga> systemSagas;
        private readonly IConsumer<ISystemPort> systemPorts;
        private readonly IConsumer<ISystemTrigger> systemTriggers;
        private readonly IConsumer<ISystemProjection> systemProjections;
        private readonly IConsumer<IMigrationHandler> migrations;
        private readonly IServiceProvider serviceProvider;
        private CronusHostOptions hostOptions;

        public CronusHost(
            CronusBooter booter,
            IConsumer<IApplicationService> appServices,
            IConsumer<ICronusEventStoreIndex> systemIndices,
            IConsumer<IEventStoreIndex> indices,
            IConsumer<IProjection> projections,
            IConsumer<IPort> ports,
            IConsumer<ISaga> sagas,
            IConsumer<IGateway> gateways,
            IConsumer<ITrigger> triggers,
            IConsumer<ISystemAppService> systemAppServices,
            IConsumer<ISystemSaga> systemSagas,
            IConsumer<ISystemPort> systemPorts,
            IConsumer<ISystemTrigger> systemTriggers,
            IConsumer<ISystemProjection> systemProjections,
            IConsumer<IMigrationHandler> migrations,
            IOptionsMonitor<CronusHostOptions> cronusHostOptions,
            IServiceProvider serviceProvider)
        {
            this.booter = booter;
            this.appServices = appServices ?? throw new ArgumentNullException(nameof(appServices));
            this.systemIndices = systemIndices;
            this.indices = indices;
            this.projections = projections ?? throw new ArgumentNullException(nameof(projections));
            this.ports = ports ?? throw new ArgumentNullException(nameof(ports));
            this.sagas = sagas ?? throw new ArgumentNullException(nameof(sagas));
            this.gateways = gateways ?? throw new ArgumentNullException(nameof(gateways));
            this.triggers = triggers;
            this.systemAppServices = systemAppServices;
            this.systemSagas = systemSagas;
            this.systemPorts = systemPorts;
            this.systemTriggers = systemTriggers;
            this.systemProjections = systemProjections;
            this.migrations = migrations;
            this.serviceProvider = serviceProvider;
            this.hostOptions = cronusHostOptions.CurrentValue;
            cronusHostOptions.OnChange(Changed);
        }

        public void Start()
        {
            try
            {
                CronusLogger.Configure(serviceProvider.GetService<ILoggerFactory>());

                booter.BootstrapCronus();

                if (hostOptions.ApplicationServicesEnabled) appServices.Start();
                if (hostOptions.SagasEnabled) sagas.Start();
                if (hostOptions.ProjectionsEnabled) projections.Start();
                if (hostOptions.PortsEnabled) ports.Start();
                if (hostOptions.GatewaysEnabled) gateways.Start();
                if (hostOptions.TriggersEnabled) triggers.Start();
                if (hostOptions.MigrationsEnabled) migrations.Start();

                if (hostOptions.SystemServicesEnabled)
                {
                    indices.Start();
                    systemIndices.Start();
                    systemAppServices.Start();
                    systemPorts.Start();
                    systemProjections.Start();
                    systemSagas.Start();
                    systemTriggers.Start();
                }
            }
            catch (Exception ex)
            {
                logger.CriticalException(ex, () => $"Start: {ex.Message}");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                if (hostOptions.ApplicationServicesEnabled) appServices.Stop();
                if (hostOptions.SagasEnabled) sagas.Stop();
                if (hostOptions.ProjectionsEnabled) projections.Stop();
                if (hostOptions.PortsEnabled) ports.Stop();
                if (hostOptions.GatewaysEnabled) gateways.Stop();
                if (hostOptions.TriggersEnabled) triggers.Stop();
                if (hostOptions.MigrationsEnabled) migrations.Stop();

                if (hostOptions.SystemServicesEnabled)
                {
                    systemAppServices.Stop();
                    systemPorts.Stop();
                    systemProjections.Stop();
                    systemSagas.Stop();
                    systemTriggers.Stop();
                    systemIndices.Stop();
                    indices.Stop();
                }
            }
            catch (Exception ex)
            {
                logger.CriticalException(ex, () => $"Stop: {ex.Message}");
                throw;
            }
        }

        public void Dispose() => Stop();

        private void Changed(CronusHostOptions newOptions)
        {
            if (hostOptions != newOptions)
            {
                logger.Debug(() => "Cronus host options re-loaded with {@options}", newOptions);

                Start(hostOptions, newOptions);
                Stop(hostOptions, newOptions);

                hostOptions = newOptions;
            }
        }

        private void Start(CronusHostOptions oldOptions, CronusHostOptions newOptions)
        {
            try
            {
                if (oldOptions.ApplicationServicesEnabled == false && newOptions.ApplicationServicesEnabled == true) appServices.Start();
                if (oldOptions.SagasEnabled == false && newOptions.SagasEnabled == true) sagas.Start();
                if (oldOptions.ProjectionsEnabled == false && newOptions.ProjectionsEnabled == true) projections.Start();
                if (oldOptions.PortsEnabled == false && newOptions.PortsEnabled == true) ports.Start();
                if (oldOptions.GatewaysEnabled == false && newOptions.GatewaysEnabled == true) gateways.Start();
                if (oldOptions.TriggersEnabled == false && newOptions.TriggersEnabled == true) gateways.Start();
            }
            catch (Exception ex)
            {
                logger.CriticalException(ex, () => $"Restart: {ex.Message}");
                throw;
            }
        }

        private void Stop(CronusHostOptions oldOptions, CronusHostOptions newOptions)
        {
            try
            {
                if (oldOptions.ApplicationServicesEnabled == true && newOptions.ApplicationServicesEnabled == false) appServices.Stop();
                if (oldOptions.SagasEnabled == true && newOptions.SagasEnabled == false) sagas.Stop();
                if (oldOptions.ProjectionsEnabled == true && newOptions.ProjectionsEnabled == false) projections.Stop();
                if (oldOptions.PortsEnabled == true && newOptions.PortsEnabled == false) ports.Stop();
                if (oldOptions.GatewaysEnabled == true && newOptions.GatewaysEnabled == false) gateways.Stop();
                if (oldOptions.TriggersEnabled == true && newOptions.TriggersEnabled == false) gateways.Stop();
            }
            catch (Exception ex)
            {
                logger.CriticalException(ex, () => $"Restop: {ex.Message}");
                throw;
            }
        }
    }
}

