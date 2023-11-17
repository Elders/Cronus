using Elders.Cronus.Multitenancy;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections
{
    internal class CronusProjectionBootstrapper
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ProjectionFinderViaReflection projectionFinderViaReflection;
        private readonly LatestProjectionVersionFinder projectionFinder;
        private readonly IPublisher<ICommand> publisher;
        private readonly CronusHostOptions cronusHostOptions;
        private readonly TenantsOptions tenants;

        public CronusProjectionBootstrapper(IServiceProvider serviceProvider, ProjectionFinderViaReflection projectionFinderViaReflection, LatestProjectionVersionFinder projectionFinder, IOptions<CronusHostOptions> cronusHostOptions, IOptions<TenantsOptions> tenantsOptions, IPublisher<ICommand> publisher)
        {
            this.serviceProvider = serviceProvider;
            this.projectionFinderViaReflection = projectionFinderViaReflection;
            this.projectionFinder = projectionFinder;
            this.publisher = publisher;
            this.cronusHostOptions = cronusHostOptions.Value;
            this.tenants = tenantsOptions.Value;
        }

        public async Task BootstrapAsync()
        {
            foreach (var tenant in tenants.Tenants)
            {
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
        }
    }
}
