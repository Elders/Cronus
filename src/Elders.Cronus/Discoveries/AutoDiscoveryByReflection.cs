using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Elders.Cronus.Logging;

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

        public virtual string Name { get { return this.GetType().Name; } }

        public DiscoveryResult Discover()
        {
            DiscoveryContext context = new DiscoveryContext();
            context.Assemblies = assemblies.Values;
            return DiscoverFromAssemblies(context);
        }

        /// <summary>
        /// Do your specific discovery logic here. You get a list of assemblies which this discovery is interested in based on the <see cref="GetInterestedTypes"/>
        /// Usually you do configure the IOC using the ISettingsBuilder.
        /// </summary>
        protected abstract DiscoveryResult DiscoverFromAssemblies(DiscoveryContext context);

        static void LoadAssembliesFromDirecotry(string directoryWithAssemblies)
        {
            var files = directoryWithAssemblies.GetFiles(new[] { "*.exe", "*.dll" });
            foreach (var assemblyFile in files)
            {
                if (assemblyFile.Contains("microsoft")) continue;

                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => x.IsDynamic == false)
                    .Where(x => x.Location.Equals(assemblyFile, StringComparison.OrdinalIgnoreCase) || x.CodeBase.Equals(assemblyFile, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();

                if (assembly == null)
                {
                    assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
                    //byte[] assemblyRaw = File.ReadAllBytes(assemblyFile);
                    //assembly = Assembly.ReflectionOnlyLoad(assemblyRaw);
                    //assembly = AppDomain.CurrentDomain.Load(assembly.GetName());
                }

                if (IsForceLoadAssemblyTypesSuccessful(assembly))
                    assemblies.Add(assembly.FullName, assembly);
            }
        }

        static void InitAssemblies()
        {
            if (1 == Interlocked.Exchange(ref shouldLoadAssembliesFromDir, 0))
            {
                string codeBase = Assembly.GetEntryAssembly().CodeBase;
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
