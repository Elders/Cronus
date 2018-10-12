using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Elders.Cronus.Logging;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Discoveries
{
    public interface IHaveConfiguration
    {
        IConfiguration Configuration { get; set; }
    }

    public abstract class DiscoveryBasedOnExecutingDirAssemblies<T> : IDiscovery<T>, IHaveConfiguration
    {
        static readonly ILog log = LogProvider.GetLogger(nameof(DiscoveryBasedOnExecutingDirAssemblies<T>));

        static DiscoveryBasedOnExecutingDirAssemblies()
        {
            InitAssemblies();
        }

        static int shouldLoadAssembliesFromDir = 1;
        static IDictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        public virtual string Name { get { return this.GetType().Name; } }

        public IConfiguration Configuration { get; set; }

        public IDiscoveryResult<T> Discover()
        {
            DiscoveryContext context = new DiscoveryContext();
            context.Configuration = Configuration;
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
                if (assemblyFile.ToLower().Contains("api-ms-win")) continue;
                if (assemblyFile.ToLower().Contains("clrcompression")) continue;

                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => x.IsDynamic == false)
                    .Where(x => x.Location.Equals(assemblyFile, StringComparison.OrdinalIgnoreCase) || x.CodeBase.Equals(assemblyFile, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();

                if (assembly == null)
                {
                    try
                    {
                        assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
                        //byte[] assemblyRaw = File.ReadAllBytes(assemblyFile);
                        //assembly = Assembly.ReflectionOnlyLoad(assemblyRaw);
                        //assembly = AppDomain.CurrentDomain.Load(assembly.GetName());
                    }
                    catch (BadImageFormatException ex)
                    {
                        log.WarnException($"Unable to load assembly {assemblyFile} which is not a bad thing at all", ex);
                    }
                }

                if (assembly is null == false)
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
