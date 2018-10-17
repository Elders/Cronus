using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Elders.Cronus.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public interface IHaveServiceCollection
    {
        IServiceCollection Services { get; set; }
    }

    public interface IHaveConfiguration
    {
        IConfiguration Configuration { get; set; }
    }

    public abstract class DiscoveryBasedOnExecutingDirAssemblies<TCronusService> : IDiscovery<TCronusService>, IHaveConfiguration
    //where TCronusService : ICronusService
    {
        static readonly ILog log = LogProvider.GetLogger(nameof(DiscoveryBasedOnExecutingDirAssemblies<TCronusService>));

        public virtual string Name { get { return this.GetType().Name; } }

        public IConfiguration Configuration { get; set; }
        public IServiceCollection Services { get; set; }

        public IDiscoveryResult<TCronusService> Discover()
        {
            DiscoveryContext context = new DiscoveryContext();
            context.Configuration = Configuration;
            context.Assemblies = AssemblyLoader.assemblies.Values;
            var discoveryResult = DiscoverFromAssemblies(context);
            return discoveryResult;
        }

        protected abstract DiscoveryResult<TCronusService> DiscoverFromAssemblies(DiscoveryContext context);
    }
}
