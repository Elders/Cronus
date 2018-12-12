using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class ProjectionsBooter : DiscoveryBase<ICronusHost>
    {
        protected override DiscoveryResult<ICronusHost> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<ICronusHost>(GetModels(context));
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(ICronusHost), typeof(CronusHost), ServiceLifetime.Transient);

            yield return new DiscoveredModel(typeof(Cronus.ProjectionsBooter), typeof(Cronus.ProjectionsBooter), ServiceLifetime.Transient);

            yield return new DiscoveredModel(typeof(BoundedContext), typeof(BoundedContext), ServiceLifetime.Transient);

            var loadedTypes = context.Assemblies.SelectMany(asm => asm.GetLoadableTypes())
                .Where(type => type.IsAbstract == false && type.IsInterface == false && typeof(IEvent).IsAssignableFrom(type) && type != typeof(EntityEvent));
            yield return new DiscoveredModel(typeof(TypeContainer<IEvent>), new TypeContainer<IEvent>(loadedTypes));
        }
    }
}
