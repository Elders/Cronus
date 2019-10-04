using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Elders.Cronus.Discoveries
{
    public interface IHaveConfiguration
    {
        IConfiguration Configuration { get; set; }
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
