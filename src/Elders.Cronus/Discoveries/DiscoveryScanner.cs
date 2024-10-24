using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Discoveries;

public sealed class DiscoveryScanner
{
    private static readonly ILogger logger = CronusLogger.CreateLogger<DiscoveryScanner>();

    public IEnumerable<IDiscoveryResult<object>> Scan(DiscoveryContext context)
    {
        IEnumerable<Type> allTypes = context.Assemblies
               .SelectMany(asm => asm
                   .GetLoadableTypes()
                   .Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery<object>).IsAssignableFrom(type)));

        IEnumerable<IDiscovery<object>> discoveries = allTypes
            .Where(candidate => allTypes.Where(t => t.BaseType == candidate).Any() == false) // filter out discoveries which inherit from each other. We remove the base discoveries
            .Select(dt => (IDiscovery<object>)FastActivator.CreateInstance(dt));

        foreach (var discovery in discoveries)
        {
            logger.LogInformation("Discovered {name}.", discovery.Name);

            yield return discovery.Discover(context);
        }
    }
}
