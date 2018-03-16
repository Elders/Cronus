using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Discoveries
{
    public abstract class AutoDiscoveryByReflection : IDiscovery
    {
        static List<Assembly> loadedAssemblies;

        public AutoDiscoveryByReflection()
        {
            if (loadedAssemblies == null)
                loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(ass => ass.IsDynamic == false).ToList();
        }

        protected abstract void DiscoverFromAssemblies(ISettingsBuilder builder, List<Assembly> loadedAssemblies);

        public void Discover(ISettingsBuilder builder)
        {
            if (ReferenceEquals(null, builder)) throw new ArgumentNullException(nameof(builder));

            DiscoverFromAssemblies(builder, loadedAssemblies);
        }
    }
}
