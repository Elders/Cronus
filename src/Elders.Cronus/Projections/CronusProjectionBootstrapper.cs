using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elders.Cronus.Projections;

internal class CronusProjectionBootstrapper
{
    private readonly IServiceProvider serviceProvider;
    private readonly ProjectionFinderViaReflection projectionFinderViaReflection;
    private readonly IPublisher<ICommand> publisher;
    private readonly ILogger<CronusProjectionBootstrapper> logger;
    private CronusHostOptions cronusHostOptions;
    private TenantsOptions tenants;

    public CronusProjectionBootstrapper(IServiceProvider serviceProvider, ProjectionFinderViaReflection projectionFinderViaReflection, IOptionsMonitor<CronusHostOptions> cronusHostOptions, IOptionsMonitor<TenantsOptions> tenantsOptions, IPublisher<ICommand> publisher, IOptionsMonitor<CronusHostOptions> cronusOptionsMonitor, ILogger<CronusProjectionBootstrapper> logger)
    {
        this.serviceProvider = serviceProvider;
        this.projectionFinderViaReflection = projectionFinderViaReflection;
        this.publisher = publisher;
        this.cronusHostOptions = cronusHostOptions.CurrentValue;
        this.tenants = tenantsOptions.CurrentValue;
        this.logger = logger;

        cronusHostOptions.OnChange(CronusHostOptionsChanged);

        tenantsOptions.OnChange(async newOptions =>
        {
            await OptionsChangedBootstrapProjectionsForTenantAsync(newOptions);
        });
    }

    public async Task BootstrapAsync()
    {
        if (cronusHostOptions.ProjectionsEnabled == false)
            return;

        List<Task> tenantBootstrapTasks = new List<Task>();
        foreach (var tenant in tenants.Tenants)
        {
            string scopedTenant = tenant;
            tenantBootstrapTasks.Add(BootstrapProjectionsForTenantAsync(scopedTenant));
        }

        await Task.WhenAll(tenantBootstrapTasks);
    }

    private async Task BootstrapProjectionsForTenantAsync(string tenant)
    {
        if (cronusHostOptions.ProjectionsEnabled == false)
            return;

        using (var scopedServiceProvider = serviceProvider.CreateScope())
        {
            var cronusContextFactory = scopedServiceProvider.ServiceProvider.GetRequiredService<Elders.Cronus.MessageProcessing.DefaultCronusContextFactory>();
            var cronusContext = cronusContextFactory.Create(tenant, scopedServiceProvider.ServiceProvider);

            IInitializableProjectionStore storeInitializer = scopedServiceProvider.ServiceProvider.GetRequiredService<IInitializableProjectionStore>();
            LatestProjectionVersionFinder finder = serviceProvider.GetRequiredService<LatestProjectionVersionFinder>();

            foreach (ProjectionVersion viaReflection in finder.GetProjectionVersionsToBootstrap())
            {
                await storeInitializer.InitializeAsync(viaReflection).ConfigureAwait(false);
            }

            await Task.Delay(5000).ConfigureAwait(false); // Enjoying the song => https://www.youtube.com/watch?v=t2nopZVrTH0

            if (cronusHostOptions.SystemServicesEnabled)
            {
                foreach (ProjectionVersion projectionVersion in projectionFinderViaReflection.GetProjectionVersionsToBootstrap())
                {
                    var id = new ProjectionVersionManagerId(projectionVersion.ProjectionName, tenant);
                    var command = new RegisterProjection(id, projectionVersion.Hash);
                    publisher.Publish(command);
                }
            }
        }
    }

    private async Task OptionsChangedBootstrapProjectionsForTenantAsync(TenantsOptions newOptions)
    {
        if (tenants.Tenants.SequenceEqual(newOptions.Tenants) == false) // Check for difference between tenants and newOptions
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Cronus host options re-loaded with {@options}", newOptions);

            // Find the difference between the old and new tenants
            // and bootstrap the new tenants
            var newTenants = newOptions.Tenants.Except(tenants.Tenants);
            List<Task> tenantBootstrapTasks = new List<Task>();
            foreach (var tenant in newTenants)
            {
                string scopedTenant = tenant;
                tenantBootstrapTasks.Add(BootstrapProjectionsForTenantAsync(scopedTenant));
            }

            await Task.WhenAll(tenantBootstrapTasks);

            tenants = newOptions;
        }
    }

    private void CronusHostOptionsChanged(CronusHostOptions newOptions)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Cronus host options re-loaded with {@options}", newOptions);

        cronusHostOptions = newOptions;

    }
}
