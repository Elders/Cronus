using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Discoveries
{
    public static class DiscoveryExtensions
    {
        public static T WithDiscovery<T>(this T self) where T : ICronusSettings
        {
            foreach (var discovery in DiscoveryFinder.Find())
            {
                discovery.Discover(self as ISettingsBuilder);
            }

            return self;
        }
    }
}
