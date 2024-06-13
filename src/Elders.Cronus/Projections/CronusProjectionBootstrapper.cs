using System;
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
    private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(CronusProjectionBootstrapper));

    private readonly IServiceProvider serviceProvider;
    private readonly ProjectionFinderViaReflection projectionFinderViaReflection;
    private readonly LatestProjectionVersionFinder projectionFinder;
    private readonly IPublisher<ICommand> publisher;
    private CronusHostOptions cronusHostOptions;
    private TenantsOptions tenants;

    public CronusProjectionBootstrapper(IServiceProvider serviceProvider, ProjectionFinderViaReflection projectionFinderViaReflection, LatestProjectionVersionFinder projectionFinder, IOptionsMonitor<CronusHostOptions> cronusHostOptions, IOptionsMonitor<TenantsOptions> tenantsOptions, IPublisher<ICommand> publisher, IOptionsMonitor<CronusHostOptions> cronusOptionsMonitor)
    {
        this.serviceProvider = serviceProvider;
        this.projectionFinderViaReflection = projectionFinderViaReflection;
        this.projectionFinder = projectionFinder;
        this.publisher = publisher;
        this.cronusHostOptions = cronusHostOptions.CurrentValue;
        this.tenants = tenantsOptions.CurrentValue;

        cronusHostOptions.OnChange(CronusHostOptionsChanged);

        tenantsOptions.OnChange(async newOptions =>
        {
            await BootstrapProjectionsForTenantAsync(newOptions);
        });
    }

    public async Task BootstrapAsync()
    {
        if (cronusHostOptions.ProjectionsEnabled == false)
            return;

        foreach (var tenant in tenants.Tenants)
        {
            await BootstrapProjectionsForTenantAsync(tenant).ConfigureAwait(false);
        }
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

            foreach (ProjectionVersion viaReflection in projectionFinder.GetProjectionVersionsToBootstrap())
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

    private async Task BootstrapProjectionsForTenantAsync(TenantsOptions newOptions)
    {
        try
        {
            if (tenants.Tenants.SequenceEqual(newOptions.Tenants) == false) // Check for difference between tenants and newOptions
            {
                logger.Info(() => "Cronus tenants options re-loaded with {@options}", newOptions);

                // Find the difference between the old and new tenants
                // and bootstrap the new tenants
                var diff = newOptions.Tenants.Except(tenants.Tenants);
                foreach (var tenant in diff)
                {
                    await BootstrapProjectionsForTenantAsync(tenant).ConfigureAwait(false);
                }

                tenants = newOptions;
            }
        }
        catch (Exception ex) when (logger.CriticalException(ex, () => $"There was an error while changing {nameof(TenantsOptions)}")) { }
    }

    private void CronusHostOptionsChanged(CronusHostOptions newOptions)
    {
        try
        {
            logger.Info(() => "Cronus host options re-loaded with {@options}", newOptions);

            cronusHostOptions = newOptions;
        }
        catch (Exception ex) when (logger.CriticalException(ex, () => $"There was an error while changing {nameof(CronusHostOptions)}")) { }
    }
}
