using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Projections;
using Elders.Cronus.Projections.InMemory;
using Elders.Cronus.Projections.Snapshotting;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class ProjectionsDiscovery : HandlersDiscovery<IProjection>
    {
        protected override IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            var models = base.GetModels(context).Concat(GetModels()).ToList();
            models.Add(new DiscoveredModel(typeof(ProjectionHasher), typeof(ProjectionHasher), ServiceLifetime.Singleton));

            return models;
        }

        IEnumerable<DiscoveredModel> GetModels()
        {
            yield return new DiscoveredModel(typeof(IProjectionReader), typeof(ProjectionRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IProjectionWriter), typeof(ProjectionRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionRepository), typeof(ProjectionRepository), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IProjectionStore), typeof(InMemoryProjectionStore), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(IInitializableProjectionStore), typeof(NotInitializableProjectionStore), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(ISnapshotStore), typeof(InMemorySnapshotStore), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(ISnapshotStrategy), typeof(NoSnapshotStrategy), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(ProjectionRepositoryWithFallback<>), typeof(ProjectionRepositoryWithFallback<>), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionRepositoryWithFallback<,>), typeof(ProjectionRepositoryWithFallback<,>), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(InMemoryProjectionVersionStore), typeof(InMemoryProjectionVersionStore), ServiceLifetime.Singleton);
        }
    }
}
