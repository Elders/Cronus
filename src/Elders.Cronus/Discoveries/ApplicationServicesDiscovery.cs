using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public abstract class HandlersDiscovery<T> : DiscoveryBasedOnExecutingDirAssemblies<T>
    {
        protected override DiscoveryResult<T> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<T>(GetModels(context));
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            var loadedTypes = context.Assemblies.SelectMany(asm => asm.GetLoadableTypes())
                .Where(type => type.IsAbstract == false && type.IsInterface == false && typeof(T).IsAssignableFrom(type));

            foreach (var type in loadedTypes)
            {
                yield return new DiscoveredModel(type, type, ServiceLifetime.Transient);
            }

            yield return new DiscoveredModel(typeof(TypeContainer<T>), new TypeContainer<T>(loadedTypes));
            yield return new DiscoveredModel(typeof(IHandlerFactory), provider => new DefaultHandlerFactory(type => provider.GetRequiredService(type)), ServiceLifetime.Transient);
        }
    }

    public class ApplicationServicesDiscovery : HandlersDiscovery<IApplicationService> { }

    public class ProjectionsDiscovery : HandlersDiscovery<IProjection> { }

    public class PortsDiscovery : HandlersDiscovery<IPort> { }

    public class SagasDiscovery : HandlersDiscovery<ISaga> { }

    public class GatewaysDiscovery : HandlersDiscovery<IGateway> { }
}
