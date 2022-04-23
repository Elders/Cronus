﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elders.Cronus.Discoveries
{
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
            Func<Type, bool> predicate = type => ;
            return assemblies
                .SelectMany(asm => asm.GetLoadableTypes())
                .Where(TypeIsNotAbstract)
                .Where(TypeIsNotInterface)
                .Where(type => typeof(TService).IsAssignableFrom(type));
        }

        private static bool TypeIsNotAbstract(Type type) => type.IsAbstract == false;
        private static bool TypeIsNotInterface(Type type) => type.IsInterface == false;
    }
}
