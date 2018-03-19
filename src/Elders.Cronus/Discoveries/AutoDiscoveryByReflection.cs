using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Discoveries
{
    public abstract class AutoDiscoveryBasedOnReferencedAssemblies : IDiscovery
    {
        public void Discover(ISettingsBuilder builder)
        {
            if (ReferenceEquals(null, builder)) throw new ArgumentNullException(nameof(builder));
            var assmemblies = this.GetAssembliesThatUse(GetInterestedTypes());
            DiscoverFromAssemblies(builder, assmemblies);
        }

        /// <summary>
        /// Do your specific discovery logic here. You get a list of assemblies which this discovery is interested in based on the <see cref="GetInterestedTypes"/>
        /// Usually do you configure the IOC using the ISettingsBuilder.
        /// </summary>
        /// <param name="builder">Cronus configuration builder. Contains Container property to .Register<T>() whatever you need</param>
        /// <param name="assemblies">List of assemblies to inspect</param>
        protected abstract void DiscoverFromAssemblies(ISettingsBuilder builder, IEnumerable<Assembly> assemblies);

        /// <summary>
        /// Defines types which this discovery is interested in. This is a shorthand for getting assemblies which the discovery will inspect
        /// </summary>
        /// <returns>Returns list of types to find assemblies</returns>
        protected abstract IEnumerable<Type> GetInterestedTypes();

        IEnumerable<Assembly> GetAssembliesThatUse(IEnumerable<Type> types)
        {
            var entry = Assembly.GetEntryAssembly();
            var referencedAssemblies = types.Select(x => x.Assembly.FullName);
            var assemblies = entry.GetReferencedAssemblies();
            foreach (var item in assemblies)
            {
                var reffedAssembly = Assembly.ReflectionOnlyLoad(item.FullName);
                var isOurAssembly = reffedAssembly.GetReferencedAssemblies().Any(x => referencedAssemblies.Contains(x.FullName));
                if (isOurAssembly)
                {
                    yield return Assembly.Load(item.FullName);
                }
            }
        }
    }

    public class FindDiscoveries : AutoDiscoveryBasedOnReferencedAssemblies
    {
        protected override void DiscoverFromAssemblies(ISettingsBuilder builder, IEnumerable<Assembly> assemblies)
        {
            var discoveries = assemblies.Where(ass => ass.IsDynamic == false)
                .SelectMany(ass => ass.GetExportedTypes().Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery).IsAssignableFrom(type) && type != typeof(FindDiscoveries)))
                .Select(dt => (IDiscovery)FastActivator.CreateInstance(dt))
                .ToList();

            discoveries.ForEach(x => x.Discover(builder));
        }

        protected override IEnumerable<Type> GetInterestedTypes()
        {
            yield return typeof(IDiscovery);
        }
    }
}
