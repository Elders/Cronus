//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Threading;

//namespace Elders.Cronus.Discoveries
//{
//    public class DiscoveryFinder
//    {
//        static int shouldLoadAssembliesFromDir = 1;
//        static List<Assembly> assemblies = new List<Assembly>();
//        private static Dictionary<string, Assembly> loadedAssmblies = new Dictionary<string, Assembly>();

//        static DiscoveryFinder()
//        {
//            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
//        }

//        public static IEnumerable<IDiscovery> Find()
//        {
//            if (1 == Interlocked.Exchange(ref shouldLoadAssembliesFromDir, 0))
//            {
//                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
//                UriBuilder uri = new UriBuilder(codeBase);
//                string path = Uri.UnescapeDataString(uri.Path);
//                var dir = Path.GetDirectoryName(path);
//                LoadAssembliesInDirecotry(dir);
//            }

//            return assemblies
//                .Where(ass => ass.IsDynamic == false)
//                .SelectMany(ass => ass.GetExportedTypes().Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery).IsAssignableFrom(type)))
//                .Select(dt => (IDiscovery)FastActivator.CreateInstance(dt));
//        }

//        static void LoadAssembliesInDirecotry(string directoryWithAssemblies)
//        {
//            var directory = new DirectoryInfo(directoryWithAssemblies);
//            foreach (var assemblyFile in directory.GetFiles().Where(x => (x.Extension == ".exe" || x.Extension == ".dll") && (x.Name.Contains("DocumentDB.Spatial.Sql") == false)))
//            {
//                byte[] assemblyRaw = File.ReadAllBytes(assemblyFile.FullName);
//                var assembly = Assembly.ReflectionOnlyLoad(assemblyRaw);
//                loadedAssmblies.Add(assembly.FullName, assembly);
//                assemblies.Add(assembly);
//            }
//        }
//        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
//        {
//            if (loadedAssmblies.ContainsKey(args.Name))
//                return loadedAssmblies[args.Name];
//            else
//                return null;
//        }
//    }

//}
