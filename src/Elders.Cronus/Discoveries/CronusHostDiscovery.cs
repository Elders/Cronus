using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class CronusHostDiscovery : DiscoveryBasedOnExecutingDirAssemblies<ICronusHost>
    {
        protected override DiscoveryResult<ICronusHost> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<ICronusHost>(GetModels(context));
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(ICronusHost), typeof(CronusHost), ServiceLifetime.Transient);

            yield return new DiscoveredModel(typeof(ApplicationServicesStartup), typeof(ApplicationServicesStartup), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ProjectionsStartup), typeof(ProjectionsStartup), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(PortsStartup), typeof(PortsStartup), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(SagasStartup), typeof(SagasStartup), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(GatewaysStartup), typeof(GatewaysStartup), ServiceLifetime.Transient);

            yield return new DiscoveredModel(typeof(BoundedContext), typeof(BoundedContext), ServiceLifetime.Transient);

            var loadedTypes = context.Assemblies.SelectMany(asm => asm.GetLoadableTypes())
                .Where(type => type.IsAbstract == false && type.IsInterface == false && typeof(IEvent).IsAssignableFrom(type) && type != typeof(EntityEvent));
            yield return new DiscoveredModel(typeof(TypeContainer<IEvent>), new TypeContainer<IEvent>(loadedTypes));
        }
    }
}
