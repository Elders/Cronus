using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Discoveries
{
    public static class DiscoveryExtensions
    {
        public static T WithDiscovery<T>(this T self) where T : ICronusSettings
        {
            var discoveryFinder = new FindDiscoveries();
            discoveryFinder.Discover(self as ISettingsBuilder);

            return self;
        }
    }
}
