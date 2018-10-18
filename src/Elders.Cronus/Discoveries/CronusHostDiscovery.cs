using System.Collections.Generic;
using Elders.Cronus.Projections.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public abstract class CronusHostDiscovery : DiscoveryBasedOnExecutingDirAssemblies<ICronusHost>
    {
        protected override DiscoveryResult<ICronusHost> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<ICronusHost>(GetModels(context));
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(ICronusHost), typeof(CronusHost), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionsBootstrapper), typeof(ProjectionsBootstrapper), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionHasher), typeof(ProjectionHasher), ServiceLifetime.Singleton);
        }
    }
}
