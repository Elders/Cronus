using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    internal class AssemblyLoader
    {
        static readonly ILogger logger = CronusLogger.CreateLogger(nameof(AssemblyLoader));

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

                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => x.IsDynamic == false)
                    .Where(x => x.Location.Equals(lowerAssemblyFile, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var assembly = loadedAssemblies.FirstOrDefault();

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

            logger.Info(() => loadAssembliesLog.ToString());
        }

        static void InitAssemblies()
        {
            if (1 == Interlocked.Exchange(ref shouldLoadAssembliesFromDir, 0))
            {
                // https://github.com/Elders/Cronus/issues/187
                // Discoveries not working when tests are using TestHost on .net core 3.0 and up
                // From.net core 3.0 the VS Test Platform(the one which is used from the Test Explorer of the VS) whenever uses a TestHost for
                // making unit testing of an asp.net core project, instead of initiating the testing from a 'testhost.dll' which is in the root
                // directory, it starts from a 'testhostx86.dll' which is located in the 'packages' folder.This breakes the discoveries as they
                // depend the initial Assembly to always be situated in the root directory of the project.This prevents Cronus from working in
                // such a situations.
                // If you use MSTest(run dotnet test instead of 'dotnet vstest') everything works because it actually runs as previous
                // (testhost.dll situated inside the correct directory)
                //string codeBase = AppDomain.CurrentDomain.BaseDirectory;
                // We need to figure out another way for the testhostx86.dll problem

                string codeBase = Assembly.GetEntryAssembly().Location;
                string path = Uri.UnescapeDataString(codeBase);
                var dir = Path.GetDirectoryName(path);
                LoadAssembliesFromDirecotry(dir);
            }
        }
    }
}
