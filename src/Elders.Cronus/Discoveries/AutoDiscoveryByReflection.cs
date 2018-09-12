using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Elders.Cronus.Logging;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Discoveries
{
    public abstract class DiscoveryBasedOnExecutingDirAssemblies : IDiscovery
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(DiscoveryBasedOnExecutingDirAssemblies));

        static DiscoveryBasedOnExecutingDirAssemblies()
        {
            InitAssemblies();
        }

        static int shouldLoadAssembliesFromDir = 1;
        static IDictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        public void Discover(ISettingsBuilder builder)
        {
            if (ReferenceEquals(null, builder)) throw new ArgumentNullException(nameof(builder));

            DiscoverFromAssemblies(builder, assemblies.Values);
        }

        /// <summary>
        /// Do your specific discovery logic here. You get a list of assemblies which this discovery is interested in based on the <see cref="GetInterestedTypes"/>
        /// Usually you do configure the IOC using the ISettingsBuilder.
        /// </summary>
        /// <param name="builder">Cronus configuration builder. Contains Container property to .Register<T>() whatever you need</param>
        /// <param name="assemblies">List of assemblies to inspect</param>
        protected abstract void DiscoverFromAssemblies(ISettingsBuilder builder, IEnumerable<Assembly> assemblies);

        static void LoadAssembliesFromDirecotry(string directoryWithAssemblies)
        {
            foreach (var assemblyFile in directoryWithAssemblies.GetFiles(new[] { "*.exe", "*.dll" }))
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => x.IsDynamic == false)
                    .Where(x => x.Location.Equals(assemblyFile, StringComparison.OrdinalIgnoreCase) || x.CodeBase.Equals(assemblyFile, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();

                if (assembly == null)
                {
                    byte[] assemblyRaw = File.ReadAllBytes(assemblyFile);
                    assembly = Assembly.ReflectionOnlyLoad(assemblyRaw);
                    assembly = AppDomain.CurrentDomain.Load(assembly.GetName());
                }

                if (IsForceLoadAssemblyTypesSuccessful(assembly))
                    assemblies.Add(assembly.FullName, assembly);
            }
        }

        static void InitAssemblies()
        {
            if (1 == Interlocked.Exchange(ref shouldLoadAssembliesFromDir, 0))
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                var dir = Path.GetDirectoryName(path);
                LoadAssembliesFromDirecotry(dir);
            }
        }

        /// <summary>
        /// Sometimes the assembly is loaded but if there are mixed or wrong dependencies TypeLoadException is thrown.
        /// So we try to load all types once during initial load and do not let such assemblies to be used.
        /// </summary>
        /// <param name="assembly">The assembly with to force</param>
        static bool IsForceLoadAssemblyTypesSuccessful(Assembly assembly)
        {
            try
            {
                List<Type> exportedTypes = assembly.GetExportedTypes().ToList();
                return true;
            }
            catch (TypeLoadException ex)
            {
                log.WarnException($"Unable to do discovery from assembly {assembly.FullName}", ex);
                return false;
            }
        }
    }
}
