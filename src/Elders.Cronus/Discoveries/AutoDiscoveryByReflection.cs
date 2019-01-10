using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Elders.Cronus.Discoveries
{
    public interface IHaveConfiguration
    {
        IConfiguration Configuration { get; set; }
    }

    [Obsolete("Use DiscoveryBase instead. Will be removed in v6.0.0")]
    public abstract class DiscoveryBasedOnExecutingDirAssemblies<TCronusService> : IDiscovery<TCronusService>, IHaveConfiguration
    //where TCronusService : ICronusService
    {
        public virtual string Name { get { return this.GetType().Name; } }

        public IConfiguration Configuration { get; set; }
        public IServiceCollection Services { get; set; }

        public IDiscoveryResult<TCronusService> Discover()
        {
            DiscoveryContext context = new DiscoveryContext();
            context.Configuration = Configuration;
            context.Assemblies = AssemblyLoader.Assemblies.Values;
            var discoveryResult = DiscoverFromAssemblies(context);
            return discoveryResult;
        }

        protected abstract DiscoveryResult<TCronusService> DiscoverFromAssemblies(DiscoveryContext context);
    }

    public abstract class DiscoveryBase<TCronusService> : IDiscovery<TCronusService>, IHaveConfiguration
    //where TCronusService : ICronusService
    {
        public virtual string Name { get { return this.GetType().Name; } }

        public IConfiguration Configuration { get; set; }
        public IServiceCollection Services { get; set; }

        public IDiscoveryResult<TCronusService> Discover()
        {
            DiscoveryContext context = new DiscoveryContext();
            context.Configuration = Configuration;
            context.Assemblies = AssemblyLoader.Assemblies.Values;
            var discoveryResult = DiscoverFromAssemblies(context);
            return discoveryResult;
        }

        protected abstract DiscoveryResult<TCronusService> DiscoverFromAssemblies(DiscoveryContext context);
    }
}
