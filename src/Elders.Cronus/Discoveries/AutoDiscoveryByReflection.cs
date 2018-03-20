using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Elders.Cronus.Logging;
using Elders.Cronus.Pipeline.Config;
using Mono.Cecil;

namespace Elders.Cronus.Discoveries
{
    public abstract class DiscoveryBasedOnExecutingDirAssemblies : IDiscovery
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(DiscoveryBasedOnExecutingDirAssemblies));

        static int shouldLoadAssembliesFromDir = 1;
        static List<AssemblyDefinition> availableAssemblies = new List<AssemblyDefinition>();

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
        protected abstract void DiscoverFromAssemblies(ISettingsBuilder builder, IEnumerable<AssemblyDefinition> assemblies);

        void InitAssemblies()
        {
            if (1 == Interlocked.Exchange(ref shouldLoadAssembliesFromDir, 0))
            {

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);

                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                var dir = Path.GetDirectoryName(path);

                var files = dir.GetFiles(new[] { "*.dll", "*.exe" });
                foreach (var file in files)
                {
                    try
                    {
                        var assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(file);
                        availableAssemblies.Add(assemblyDefinition);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorException($"Unable to do discovery from assembly {file}", ex);
                    }
                }
            }
        }

        static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return System.Reflection.Assembly.ReflectionOnlyLoad(args.Name);
        }
    }

    public class FindDiscoveries : DiscoveryBasedOnExecutingDirAssemblies
    {
        protected override void DiscoverFromAssemblies(ISettingsBuilder builder, IEnumerable<AssemblyDefinition> assemblies)
        {
            var discoveries = assemblies.SelectMany(x => x.Modules)
                .SelectMany(mod => mod.GetTypes().Where(type =>
                {
                    var td = mod.ImportReference(typeof(IDiscovery)).Resolve();
                    var ignore = mod.ImportReference(typeof(FindDiscoveries)).Resolve();
                    return type.IsAbstract == false && type.IsClass && td.IsAssignableFrom(type) && type != ignore;
                }))
                .Select(dt => (IDiscovery)FastActivator.CreateInstance(dt.ToType()))
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

    static internal class TypeDefinitionExtensions
    {
        /// <summary>
        /// Is childTypeDef a subclass of parentTypeDef. Does not test interface inheritance
        /// </summary>
        /// <param name="childTypeDef"></param>
        /// <param name="parentTypeDef"></param>
        /// <returns></returns>
        public static bool IsSubclassOf(this TypeDefinition childTypeDef, TypeDefinition parentTypeDef)
        {
            return
                childTypeDef.MetadataToken != parentTypeDef.MetadataToken && childTypeDef.EnumerateBaseClasses()
                .Any(b => b.MetadataToken == parentTypeDef.MetadataToken);
        }

        /// <summary>
        /// Does childType inherit from parentInterface
        /// </summary>
        /// <param name="childType"></param>
        /// <param name="parentInterfaceDef"></param>
        /// <returns></returns>
        public static bool DoesAnySubTypeImplementInterface(this TypeDefinition childType, TypeDefinition parentInterfaceDef)
        {
            return childType
               .EnumerateBaseClasses()
               .Any(typeDefinition => typeDefinition.DoesSpecificTypeImplementInterface(parentInterfaceDef));
        }

        /// <summary>
        /// Does the childType directly inherit from parentInterface. Base
        /// classes of childType are not tested
        /// </summary>
        /// <param name="childTypeDef"></param>
        /// <param name="parentInterfaceDef"></param>
        /// <returns></returns>
        public static bool DoesSpecificTypeImplementInterface(this TypeDefinition childTypeDef, TypeDefinition parentInterfaceDef)
        {
            return childTypeDef
               .Interfaces
               .Any(ifaceDef => DoesSpecificInterfaceImplementInterface(ifaceDef.InterfaceType.Resolve(), parentInterfaceDef));
        }

        /// <summary>
        /// Does interface iface0 equal or implement interface iface1
        /// </summary>
        /// <param name="iface0"></param>
        /// <param name="iface1"></param>
        /// <returns></returns>
        public static bool DoesSpecificInterfaceImplementInterface(TypeDefinition iface0, TypeDefinition iface1)
        {
            return iface0.MetadataToken == iface1.MetadataToken || iface0.DoesAnySubTypeImplementInterface(iface1);
        }

        /// <summary>
        /// Is source type assignable to target type
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsAssignableFrom(this TypeDefinition target, TypeDefinition source)
        {
            return
                target == source ||
                target.MetadataToken == source.MetadataToken ||
                source.IsSubclassOf(target) ||
                target.IsInterface && source.DoesAnySubTypeImplementInterface(target);
        }

        /// <summary>
        /// Enumerate the current type, it's parent and all the way to the top type
        /// </summary>
        /// <param name="klassType"></param>
        /// <returns></returns>
        public static IEnumerable<TypeDefinition> EnumerateBaseClasses(this TypeDefinition klassType)
        {
            for (var typeDefinition = klassType; typeDefinition != null; typeDefinition = typeDefinition.BaseType?.Resolve())
            {
                yield return typeDefinition;
            }
        }

        public static Type ToType(this TypeDefinition typeDef)
        {
            return Type.GetType(typeDef.FullName + ", " + typeDef.Module.Assembly.FullName);
        }
    }
}
