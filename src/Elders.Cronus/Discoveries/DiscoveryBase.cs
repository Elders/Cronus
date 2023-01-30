using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Elders.Cronus.Discoveries;

public abstract class DiscoveryBase<TCronusService> : IDiscovery<TCronusService>    //where TCronusService : ICronusService
{
    public virtual string Name { get { return this.GetType().Name; } }

    public IDiscoveryResult<TCronusService> Discover(DiscoveryContext context)
    {
        return DiscoverFromAssemblies(context);
    }

    protected abstract DiscoveryResult<TCronusService> DiscoverFromAssemblies(DiscoveryContext context);

    protected IEnumerable<DiscoveredModel> DiscoverModel<TService, TImplementation>(ServiceLifetime lifestyle, bool replaceExistingService = false)
    {
        yield return new DiscoveredModel(typeof(TService), typeof(TImplementation), lifestyle) { CanOverrideDefaults = replaceExistingService };

        if (typeof(TService) != typeof(TImplementation))
            yield return new DiscoveredModel(typeof(TImplementation), typeof(TImplementation), lifestyle);
    }
}
