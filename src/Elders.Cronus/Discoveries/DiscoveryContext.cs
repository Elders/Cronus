using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
