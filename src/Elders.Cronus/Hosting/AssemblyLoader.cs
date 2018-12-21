using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using Elders.Cronus.Logging;

namespace Elders.Cronus
{
    internal class AssemblyLoader
    {
        static readonly ILog log = LogProvider.GetLogger(nameof(AssemblyLoader));

        static int shouldLoadAssembliesFromDir = 1;
        static string[] excludedAssemblies = new string[] { "sni.dll", "apphost.exe", "clrcompression.dll", "clretwrc.dll", "clrjit.dll", "coreclr.dll", "dbgshim.dll", "hostfxr.dll", "hostpolicy.dll", "sos.dll", "ucrtbase.dll" };
        static string[] wildcards = new string[] { "microsoft", "api-ms", "sos_", "mscordaccore", "mscor" };

        internal static IDictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

        static AssemblyLoader()
        {
            InitAssemblies();
        }

        static void LoadAssembliesFromDirecotry(string directoryWithAssemblies)
        {
            StringBuilder loadAssembliesLog = new StringBuilder();
            loadAssembliesLog.AppendLine($"Try loading assemblies from directory: {directoryWithAssemblies}");
            loadAssembliesLog.AppendLine("Some assemblies will not be loaded because they are not managed which is fine and we output them bellow if any for completeness.");

            var files = directoryWithAssemblies.GetFiles(new[] { "*.exe", "*.dll" });
            foreach (var assemblyFile in files)
            {
                var lowerAssemblyFile = assemblyFile.ToLower();
                if (wildcards.Any(x => lowerAssemblyFile.Contains(x))) continue;
                if (excludedAssemblies.Any(x => lowerAssemblyFile.EndsWith(x))) continue;

                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => x.IsDynamic == false)
                    .Where(x => x.Location.Equals(lowerAssemblyFile, StringComparison.OrdinalIgnoreCase) || x.CodeBase.Equals(lowerAssemblyFile, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();

                if (assembly is null)
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
                        loadAssembliesLog.AppendLine($"Unable to load assembly {assemblyFile}. Reason:{ex.Message}");
                    }
                }

                if (assembly is null == false)
                    Assemblies.Add(assembly.FullName, assembly);
            }

            log.Info(loadAssembliesLog.ToString());
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
