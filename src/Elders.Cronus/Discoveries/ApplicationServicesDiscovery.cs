using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public abstract class HandlersDiscovery<T> : DiscoveryBasedOnExecutingDirAssemblies<T>
    {
        protected override DiscoveryResult<T> DiscoverFromAssemblies(DiscoveryContext context)
        {
            var discoveredHandlers = context.Assemblies.SelectMany(asm => asm.GetLoadableTypes())
                .Where(type => type.IsAbstract == false && type.IsInterface == false && typeof(T).IsAssignableFrom(type))
                .Select(x => new DiscoveredModel(x, x, ServiceLifetime.Transient));

            return new DiscoveryResult<T>(discoveredHandlers);
        }
    }

    public class ApplicationServicesDiscovery : HandlersDiscovery<IAggregateRootApplicationService> { }

    public class ProjectionsDiscovery : HandlersDiscovery<IProjection> { }

    public class PortsDiscovery : HandlersDiscovery<IPort> { }

    public class SagasDiscovery : HandlersDiscovery<ISaga> { }

    public class GatewaysDiscovery : HandlersDiscovery<IGateway> { }
}
