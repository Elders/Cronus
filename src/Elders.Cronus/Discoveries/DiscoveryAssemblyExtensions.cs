using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elders.Cronus.Discoveries
{
    public static class DiscoveryAssemblyExtensions
    {
        public static IEnumerable<Type> Find<TService>(this IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(asm => asm.GetLoadableTypes())
                .Where(type => type.IsAbstract == false)
                .Where(type => type.IsInterface == false)
                .Where(type => typeof(TService).IsAssignableFrom(type));
        }
    }
}
