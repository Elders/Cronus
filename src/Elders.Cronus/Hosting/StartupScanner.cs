using Elders.Cronus.Discoveries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus
{
    public class CronusStartupScanner
    {
        public IEnumerable<Type> Scan()
        {
            var startups = AssemblyLoader.Assemblies
                .SelectMany(asm => asm.Value
                    .GetLoadableTypes()
                    .Where(type => type.IsAbstract == false && type.IsClass && typeof(ICronusStartup).IsAssignableFrom(type)));

            return startups;
        }
    }
}
