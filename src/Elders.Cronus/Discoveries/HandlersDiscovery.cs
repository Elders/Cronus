using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries;

public abstract class HandlersDiscovery<T> : DiscoveryBase<T>
{
    protected override DiscoveryResult<T> DiscoverFromAssemblies(DiscoveryContext context)
    {
        return new DiscoveryResult<T>(DiscoverHandlers(context));
    }

    protected virtual IEnumerable<DiscoveredModel> DiscoverHandlers(DiscoveryContext context)
    {
        var foundTypes = context.FindService<T>();
        foreach (var type in foundTypes)
        {
            yield return new DiscoveredModel(type, type, ServiceLifetime.Transient);
        }

        yield return new DiscoveredModel(typeof(TypeContainer<T>), new TypeContainer<T>(foundTypes));
        yield return new DiscoveredModel(typeof(IHandlerFactory), typeof(DefaultHandlerFactory), ServiceLifetime.Singleton);
        yield return new DiscoveredModel(typeof(DefaultHandlerFactory), typeof(DefaultHandlerFactory), ServiceLifetime.Singleton);
    }
}
