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

        static int shouldLoadAssembliesFromDir = 1;
        static List<Assembly> availableAssemblies = new List<Assembly>();

        public DiscoveryBasedOnExecutingDirAssemblies()
        {
            InitAssemblies();
        }

        public void Discover(ISettingsBuilder builder)
        {
            if (ReferenceEquals(null, builder)) throw new ArgumentNullException(nameof(builder));

            DiscoverFromAssemblies(builder, availableAssemblies);
        }

        /// <summary>
        /// Do your specific discovery logic here. You get a list of assemblies which this discovery is interested in based on the <see cref="GetInterestedTypes"/>
        /// Usually do you configure the IOC using the ISettingsBuilder.
        /// </summary>
        /// <param name="builder">Cronus configuration builder. Contains Container property to .Register<T>() whatever you need</param>
        /// <param name="assemblies">List of assemblies to inspect</param>
        protected abstract void DiscoverFromAssemblies(ISettingsBuilder builder, IEnumerable<Assembly> assemblies);

        void InitAssemblies()
        {
            if (1 == Interlocked.Exchange(ref shouldLoadAssembliesFromDir, 0))
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                var dir = Path.GetDirectoryName(path);

                var files = dir.GetFiles(new[] { "*.dll", "*.exe" });
                foreach (var file in files)
                {
                    try
                    {
                        var reflAssembly = Assembly.ReflectionOnlyLoadFrom(file);
                        availableAssemblies.Add(reflAssembly);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorException($"Unable to do discovery from assembly {file}", ex);
                    }
                }
            }
        }
    }

    public class FindDiscoveries : DiscoveryBasedOnExecutingDirAssemblies
    {
        protected override void DiscoverFromAssemblies(ISettingsBuilder builder, IEnumerable<Assembly> assemblies)
        {
            var discoveries = assemblies.Where(ass => ass.IsDynamic == false)
                .SelectMany(ass => ass.GetExportedTypes().Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery).IsAssignableFrom(type) && type != typeof(FindDiscoveries)))
                .Select(dt => (IDiscovery)FastActivator.CreateInstance(dt))
                .ToList();

            discoveries.ForEach(x => x.Discover(builder));
        }
    }

    public static class FileSearchExtentions
    {
        public static IEnumerable<string> GetFiles(this string path, string regexPattern = "", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            System.Text.RegularExpressions.Regex reSearchPattern = new System.Text.RegularExpressions.Regex(regexPattern);
            return Directory.EnumerateFiles(path, "*", searchOption).Where(file => reSearchPattern.IsMatch(Path.GetFileName(file)));
        }

        public static IEnumerable<string> GetFiles(this string path, string[] searchPatterns, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return searchPatterns.AsParallel().SelectMany(searchPattern => Directory.EnumerateFiles(path, searchPattern, searchOption));
        }
    }
}
