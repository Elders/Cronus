using System.Collections.Generic;
using Elders.Cronus.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class ProjectionPlayerDiscovery : DiscoveryBasedOnExecutingDirAssemblies<ProjectionPlayer>
    {
        protected override DiscoveryResult<ProjectionPlayer> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<ProjectionPlayer>(GetModels());
        }

        IEnumerable<DiscoveredModel> GetModels()
        {
            yield return new DiscoveredModel(typeof(ProjectionPlayer), typeof(ProjectionPlayer), ServiceLifetime.Transient);
        }
    }
}
