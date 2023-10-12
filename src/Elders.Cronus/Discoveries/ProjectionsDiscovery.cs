using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.Rebuilding;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class ProjectionsDiscovery : HandlersDiscovery<IProjection>
    {
        protected override IEnumerable<DiscoveredModel> DiscoverHandlers(DiscoveryContext context)
        {
            var models = base.DiscoverHandlers(context).Concat(GetModels()).ToList();
            models.Add(new DiscoveredModel(typeof(ProjectionHasher), typeof(ProjectionHasher), ServiceLifetime.Singleton));

            return models;
        }

        IEnumerable<DiscoveredModel> GetModels()
        {
            yield return new DiscoveredModel(typeof(IProjectionReader), typeof(ProjectionRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IProjectionWriter), typeof(ProjectionRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionRepository), typeof(ProjectionRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionRepositoryWithFallback<>), typeof(ProjectionRepositoryWithFallback<>), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionRepositoryWithFallback<,>), typeof(ProjectionRepositoryWithFallback<,>), ServiceLifetime.Transient);
            //yield return new DiscoveredModel(typeof(InMemoryProjectionVersionStore), typeof(InMemoryProjectionVersionStore), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(IProjectionVersioningPolicy), typeof(MarkupInterfaceProjectionVersioningPolicy), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(MarkupInterfaceProjectionVersioningPolicy), typeof(MarkupInterfaceProjectionVersioningPolicy), ServiceLifetime.Singleton);

            yield return new DiscoveredModel(typeof(CronusProjectionBootstrapper), typeof(CronusProjectionBootstrapper), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(LatestProjectionVersionFinder), typeof(LatestProjectionVersionFinder), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IProjectionVersionFinder), typeof(ProjectionFinderViaReflection), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionFinderViaReflection), typeof(ProjectionFinderViaReflection), ServiceLifetime.Transient);
        }
    }

    public class SystemdProjectionsDiscovery : HandlersDiscovery<ISystemProjection>
    {
        protected override IEnumerable<DiscoveredModel> DiscoverHandlers(DiscoveryContext context)
        {
            var models = base.DiscoverHandlers(context).Concat(GetModels()).ToList();
            models.Add(new DiscoveredModel(typeof(ProjectionHasher), typeof(ProjectionHasher), ServiceLifetime.Singleton));

            return models;
        }

        IEnumerable<DiscoveredModel> GetModels()
        {
            yield return new DiscoveredModel(typeof(IProjectionReader), typeof(ProjectionRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IProjectionWriter), typeof(ProjectionRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionRepository), typeof(ProjectionRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionRepositoryWithFallback<>), typeof(ProjectionRepositoryWithFallback<>), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionRepositoryWithFallback<,>), typeof(ProjectionRepositoryWithFallback<,>), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IProjectionVersioningPolicy), typeof(MarkupInterfaceProjectionVersioningPolicy), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(MarkupInterfaceProjectionVersioningPolicy), typeof(MarkupInterfaceProjectionVersioningPolicy), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(ProjectionVersionHelper), typeof(ProjectionVersionHelper), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProgressTracker), typeof(ProgressTracker), ServiceLifetime.Scoped);
        }
    }
}
