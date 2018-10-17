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
    internal class AssemblyLoader
    {
        static readonly ILog log = LogProvider.GetLogger(nameof(AssemblyLoader));

        internal static IDictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        static AssemblyLoader()
        {
            InitAssemblies();
        }

        static int shouldLoadAssembliesFromDir = 1;
        static string[] excludedAssemblies = new string[] { "apphost.exe", "clrcompression.dll", "clretwrc.dll", "clrjit.dll", "coreclr.dll", "dbgshim.dll", "hostfxr.dll", "hostpolicy.dll", "mscordaccore.dll", "mscordaccore_amd64_amd64_4.6.26814.03.dll", "mscordbi.dll", "mscorrc.debug.dll", "mscorrc.dll", "sos.dll", "sos_amd64_amd64_4.6.26814.03.dll", "ucrtbase.dll" };
        static string[] wildcards = new string[] { "microsoft", "api-ms" };
        static void LoadAssembliesFromDirecotry(string directoryWithAssemblies)
        {
            var files = directoryWithAssemblies.GetFiles(new[] { "*.exe", "*.dll" }).Select(filepath => filepath.ToLower());
            foreach (var assemblyFile in files)
            {
                if (wildcards.Any(x => assemblyFile.Contains(x))) continue;
                if (excludedAssemblies.Any(x => assemblyFile.EndsWith(x))) continue;

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
