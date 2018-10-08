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
    public abstract class DiscoveryBasedOnExecutingDirAssemblies<T> : IDiscovery<T>
    {
        static readonly ILog log = LogProvider.GetLogger(nameof(DiscoveryBasedOnExecutingDirAssemblies<T>));

        static DiscoveryBasedOnExecutingDirAssemblies()
        {
            InitAssemblies();
        }

        static int shouldLoadAssembliesFromDir = 1;
        static IDictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        public virtual string Name { get { return this.GetType().Name; } }

        public IDiscoveryResult<T> Discover()
        {
            DiscoveryContext context = new DiscoveryContext();
            context.Assemblies = assemblies.Values;
            return DiscoverFromAssemblies(context);
        }

        protected abstract DiscoveryResult<T> DiscoverFromAssemblies(DiscoveryContext context);

        static void LoadAssembliesFromDirecotry(string directoryWithAssemblies)
        {
            var files = directoryWithAssemblies.GetFiles(new[] { "*.exe", "*.dll" });
            foreach (var assemblyFile in files)
            {
                if (assemblyFile.ToLower().Contains("microsoft")) continue;

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
    }
}
