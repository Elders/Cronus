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
                    .Where(type => type.IsAbstract == false && type.IsClass && typeof(ICronusStartup).IsAssignableFrom(type)))
                .OrderBy(type => GetCronusStartupRank(type));

            return startups;
        }

        private int GetCronusStartupRank(Type type)
        {
            CronusStartupAttribute cronusStartupAttribute = type
                .GetCustomAttributes(typeof(CronusStartupAttribute), false)
                .SingleOrDefault() as CronusStartupAttribute;

            Bootstraps rank = Bootstraps.Runtime;

            if (cronusStartupAttribute is null == false)
                rank = cronusStartupAttribute.Bootstraps;

            return (int)rank;
        }
    }
}
