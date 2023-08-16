using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Hosting;
using Elders.Cronus.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private readonly IRpcHost rpcHost;
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
            IServiceProvider serviceProvider, IRpcHost rpcHost)
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
            this.rpcHost = rpcHost;
            this.hostOptions = cronusHostOptions.CurrentValue;
            cronusHostOptions.OnChange(Changed);
        }

        public async Task StartAsync()
        {
            try
            {
                CronusLogger.Configure(serviceProvider.GetService<ILoggerFactory>());

                booter.BootstrapCronus();

                if (hostOptions.SystemServicesEnabled)
                {
                    await systemIndices.StartAsync().ConfigureAwait(false);
                    await systemAppServices.StartAsync().ConfigureAwait(false);
                    await systemPorts.StartAsync().ConfigureAwait(false);
                    await systemProjections.StartAsync().ConfigureAwait(false);
                    await systemSagas.StartAsync().ConfigureAwait(false);
                    await systemTriggers.StartAsync().ConfigureAwait(false);
                }

                await Task.Delay(1000).ConfigureAwait(false); // There is no specific reason to have this. I am just experimenting with it if bootstrapping will be better that way. If you think we can remove it do it.

                if (hostOptions.ApplicationServicesEnabled)
                {
                    await appServices.StartAsync().ConfigureAwait(false);
                    await indices.StartAsync().ConfigureAwait(false);
                }

                if (hostOptions.SagasEnabled) await sagas.StartAsync().ConfigureAwait(false);
                if (hostOptions.ProjectionsEnabled) await projections.StartAsync().ConfigureAwait(false);
                if (hostOptions.PortsEnabled) await ports.StartAsync().ConfigureAwait(false);
                if (hostOptions.GatewaysEnabled) await gateways.StartAsync().ConfigureAwait(false);
                if (hostOptions.TriggersEnabled) await triggers.StartAsync().ConfigureAwait(false);
                if (hostOptions.MigrationsEnabled) await migrations.StartAsync().ConfigureAwait(false);

                if (hostOptions.RpcApiEnabled)
                {
                    await rpcHost.StartAsync();
                }

            }
            catch (Exception ex)
            {
                logger.CriticalException(ex, () => $"Start: {ex.Message}");
                throw;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                List<Task> stopTasks = new List<Task>();

                if (hostOptions.ApplicationServicesEnabled) stopTasks.Add(appServices.StopAsync());
                if (hostOptions.SagasEnabled) stopTasks.Add(sagas.StopAsync());
                if (hostOptions.ProjectionsEnabled) stopTasks.Add(projections.StopAsync());
                if (hostOptions.PortsEnabled) stopTasks.Add(ports.StopAsync());
                if (hostOptions.GatewaysEnabled) stopTasks.Add(gateways.StopAsync());
                if (hostOptions.TriggersEnabled) stopTasks.Add(triggers.StopAsync());
                if (hostOptions.MigrationsEnabled) stopTasks.Add(migrations.StopAsync());

                if (hostOptions.SystemServicesEnabled)
                {
                    stopTasks.Add(systemAppServices.StopAsync());
                    stopTasks.Add(systemPorts.StopAsync());
                    stopTasks.Add(systemProjections.StopAsync());
                    stopTasks.Add(systemSagas.StopAsync());
                    stopTasks.Add(systemTriggers.StopAsync());
                    stopTasks.Add(systemIndices.StopAsync());
                    stopTasks.Add(indices.StopAsync());
                }

                if (hostOptions.RpcApiEnabled)
                {
                    stopTasks.Add(rpcHost.StopAsync());
                }

                await Task.WhenAll(stopTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.CriticalException(ex, () => $"Stop: {ex.Message}");
                throw;
            }
        }

        public void Dispose() => StopAsync().GetAwaiter().GetResult();

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
                if (oldOptions.ApplicationServicesEnabled == false && newOptions.ApplicationServicesEnabled == true) appServices.StartAsync();
                if (oldOptions.SagasEnabled == false && newOptions.SagasEnabled == true) sagas.StartAsync();
                if (oldOptions.ProjectionsEnabled == false && newOptions.ProjectionsEnabled == true) projections.StartAsync();
                if (oldOptions.PortsEnabled == false && newOptions.PortsEnabled == true) ports.StartAsync();
                if (oldOptions.GatewaysEnabled == false && newOptions.GatewaysEnabled == true) gateways.StartAsync();
                if (oldOptions.TriggersEnabled == false && newOptions.TriggersEnabled == true) gateways.StartAsync();
                if (oldOptions.RpcApiEnabled == false && newOptions.RpcApiEnabled == true) rpcHost.StartAsync();
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
                if (oldOptions.ApplicationServicesEnabled == true && newOptions.ApplicationServicesEnabled == false) appServices.StopAsync();
                if (oldOptions.SagasEnabled == true && newOptions.SagasEnabled == false) sagas.StopAsync();
                if (oldOptions.ProjectionsEnabled == true && newOptions.ProjectionsEnabled == false) projections.StopAsync();
                if (oldOptions.PortsEnabled == true && newOptions.PortsEnabled == false) ports.StopAsync();
                if (oldOptions.GatewaysEnabled == true && newOptions.GatewaysEnabled == false) gateways.StopAsync();
                if (oldOptions.TriggersEnabled == true && newOptions.TriggersEnabled == false) gateways.StopAsync();
                if (oldOptions.RpcApiEnabled == true && newOptions.RpcApiEnabled == false) rpcHost.StopAsync();
            }
            catch (Exception ex)
            {
                logger.CriticalException(ex, () => $"Restop: {ex.Message}");
                throw;
            }
        }
    }
}

