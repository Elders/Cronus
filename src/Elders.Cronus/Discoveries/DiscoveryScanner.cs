using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Discoveries
{
    public sealed class DiscoveryScanner
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger<DiscoveryScanner>();

        public IEnumerable<IDiscoveryResult<object>> Scan(DiscoveryContext context)
        {
            var discoveries = context.Assemblies
                .SelectMany(asm => asm
                    .GetLoadableTypes()
                    .Where(type => type.IsAbstract == false && type.IsClass && typeof(IDiscovery<object>).IsAssignableFrom(type)))
                .Select(dt => (IDiscovery<object>)FastActivator.CreateInstance(dt));

            foreach (var discovery in discoveries)
            {
                logger.Info(() => $"Discovered {discovery.Name}");

                yield return discovery.Discover(context);
            }
        }
    }
}
