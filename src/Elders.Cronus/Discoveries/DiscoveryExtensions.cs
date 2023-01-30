using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elders.Cronus.Discoveries;

public static class DiscoveryExtensions
{
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        if (assembly is null) throw new ArgumentNullException(nameof(assembly));

        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
    }

    public static IEnumerable<Type> Find<TService>(this IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(asm => asm.GetLoadableTypes())
            .Where(TypeIsNotAbstract)
            .Where(TypeIsNotInterface)
            .Where(type => typeof(TService).IsAssignableFrom(type));
    }

    public static IEnumerable<Type> FindExcept<TService>(this IEnumerable<Assembly> assemblies, Type second)
    {
        return assemblies
             .SelectMany(asm => asm.GetLoadableTypes())
             .Where(TypeIsNotAbstract)
             .Where(TypeIsNotInterface)
             .Where(type => typeof(TService).IsAssignableFrom(type))
             .Where(type => type.IsAssignableFrom(second) == false);
    }

    private static bool TypeIsNotAbstract(Type type) => type.IsAbstract == false;
    private static bool TypeIsNotInterface(Type type) => type.IsInterface == false;
}
