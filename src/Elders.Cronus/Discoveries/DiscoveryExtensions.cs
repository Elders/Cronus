using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Discoveries
{
    public static class DiscoveryExtensions
    {
        public static T WithDiscovery<T>(this T self) where T : ICronusSettings
        {
            var discoveryFinder = new DiscoveryScanner(null);
            discoveryFinder.Discover();

            return self;
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly is null) throw new ArgumentNullException(nameof(assembly));

            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
        }
    }
}
