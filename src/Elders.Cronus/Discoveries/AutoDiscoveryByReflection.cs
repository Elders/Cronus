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
        //static List<Assembly> assemblies = new List<Assembly>();
        static IDictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        public void Discover(ISettingsBuilder builder)
        {
            if (ReferenceEquals(null, builder)) throw new ArgumentNullException(nameof(builder));

            DiscoverFromAssemblies(builder, assemblies.Values);
        }

        /// <summary>
        /// Do your specific discovery logic here. You get a list of assemblies which this discovery is interested in based on the <see cref="GetInterestedTypes"/>
        /// Usually do you configure the IOC using the ISettingsBuilder.
        /// </summary>
        /// <param name="builder">Cronus configuration builder. Contains Container property to .Register<T>() whatever you need</param>
        /// <param name="assemblies">List of assemblies to inspect</param>
        protected abstract void DiscoverFromAssemblies(ISettingsBuilder builder, IEnumerable<Assembly> assemblies);

        static void LoadAssembliesInDirecotry(string directoryWithAssemblies)
        {
            foreach (var assemblyFile in directoryWithAssemblies.GetFiles(new[] { "*.exe", "*.dll" }))
            {
                try
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(x => x.IsDynamic == false)
                        .ToList()
                        .Where(x => x.Location.Equals(assemblyFile, StringComparison.OrdinalIgnoreCase) || x.CodeBase.Equals(assemblyFile, StringComparison.OrdinalIgnoreCase))
                        .SingleOrDefault();

                    if (assembly == null)
                    {
                        byte[] assemblyRaw = File.ReadAllBytes(assemblyFile);
                        assembly = Assembly.ReflectionOnlyLoad(assemblyRaw);
                        assembly = AppDomain.CurrentDomain.Load(assembly.GetName());
                    }
                    assemblies.Add(assembly.FullName, assembly);
                    // Sometimes the assembly is loaded but if there are mixed or wrong dependencies TypeLoadException is thrown.
                    // So we try to load all types once during initial load and do not let such assemblies to be used.
                    List<Type> exportedTypes = assembly.GetExportedTypes().ToList();
                }
                catch (Exception ex)
                {
                    log.ErrorException($"Unable to do discovery from assembly {assemblyFile}", ex);
                }
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
                LoadAssembliesInDirecotry(dir);
            }
        }
    }
}
