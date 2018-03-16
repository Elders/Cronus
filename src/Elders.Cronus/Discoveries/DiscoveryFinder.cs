using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Discoveries
{
    public class DiscoveryFinder
    {
        public static IEnumerable<IDiscovery> Find()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies().Where(ass => ass.IsDynamic == false)
                .SelectMany(ass => ass.GetExportedTypes().Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery).IsAssignableFrom(type)))
                .Select(dt => (IDiscovery)FastActivator.CreateInstance(dt));
        }
    }
}
