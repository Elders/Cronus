using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Discoveries
{
    public class FindDiscoveries : DiscoveryBasedOnExecutingDirAssemblies
    {
        protected override void DiscoverFromAssemblies(ISettingsBuilder builder, IEnumerable<Assembly> assemblies)
        {
            var discoveries = assemblies
                .SelectMany(asm =>
                {
                    IEnumerable<Type> exportedTypes = asm.GetExportedTypes();
                    return exportedTypes.Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery).IsAssignableFrom(type) && type != typeof(FindDiscoveries));
                })
                .Select(dt => (IDiscovery)FastActivator.CreateInstance(dt)).ToList();

            discoveries.ForEach(x => x.Discover(builder));
        }
    }
}
