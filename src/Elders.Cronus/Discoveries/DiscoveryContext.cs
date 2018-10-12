using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Discoveries
{
    public class DiscoveryContext
    {
        public IEnumerable<Assembly> Assemblies { get; set; }

        public IConfiguration Configuration { get; set; }
    }
}
