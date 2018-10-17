using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Pipeline.Config;
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
            var handlerTypes = context.Assemblies.SelectMany(asm => asm.GetLoadableTypes())
                .Where(type => type.IsAbstract == false && type.IsInterface == false && typeof(T).IsAssignableFrom(type));

            foreach (var handlerType in handlerTypes)
            {
                yield return new DiscoveredModel(handlerType, handlerType, ServiceLifetime.Transient);
            }

            yield return new DiscoveredModel(typeof(HandlerTypeContainer<T>), new HandlerTypeContainer<T>(handlerTypes));
            yield return new DiscoveredModel(typeof(IHandlerFactory), provider => new DefaultHandlerFactory(type => provider.GetService(type)), ServiceLifetime.Singleton);
        }
    }

    public class ApplicationServicesDiscovery : HandlersDiscovery<IAggregateRootApplicationService> { }

    public class ProjectionsDiscovery : HandlersDiscovery<IProjection> { }

    public class PortsDiscovery : HandlersDiscovery<IPort> { }

    public class SagasDiscovery : HandlersDiscovery<ISaga> { }

    public class GatewaysDiscovery : HandlersDiscovery<IGateway> { }
}
