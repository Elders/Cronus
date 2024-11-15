using System;
using System.Linq;
using System.Collections.Generic;

namespace Elders.Cronus;

public class CronusStartupScanner
{
    private readonly IAssemblyScanner assemblyScanner;

    public CronusStartupScanner(IAssemblyScanner assemblyScanner)
    {
        this.assemblyScanner = assemblyScanner;
    }

    public IEnumerable<Type> Scan()
    {
        var startups = assemblyScanner
            .Scan()
            .Where(type => type.IsAbstract == false && type.IsClass && typeof(ICronusStartup).IsAssignableFrom(type))
            .OrderBy(type => GetCronusStartupRank(type));

        return startups;
    }

    public IEnumerable<Type> ScanForCronusTenantStartups()
    {
        var startups = assemblyScanner
            .Scan()
            .Where(type => type.IsAbstract == false && type.IsClass && typeof(ICronusTenantStartup).IsAssignableFrom(type))
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
