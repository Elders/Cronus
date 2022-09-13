using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Discoveries
{
    public class DiscoveryContext
    {
        public DiscoveryContext(IEnumerable<Assembly> assemblies, IConfiguration configuration)
        {
            Assemblies = assemblies;
            Configuration = configuration;
        }

        public IEnumerable<Assembly> Assemblies { get; }

        public IConfiguration Configuration { get; set; }

        public IEnumerable<Type> Types => Assemblies.SelectMany(asm => asm.GetLoadableTypes());

        public IEnumerable<Type> FindService<TService>() => Assemblies.Find<TService>();

        public IEnumerable<Type> FindServiceExcept<TService>(Type serviceType) => Assemblies.FindExcept<TService>(serviceType);
    }
}
