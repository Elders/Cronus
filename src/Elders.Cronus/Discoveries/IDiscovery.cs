using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Discoveries
{
    public interface IDiscovery
    {
        string Name { get; }

        DiscoveryResult Discover();
    }
}
