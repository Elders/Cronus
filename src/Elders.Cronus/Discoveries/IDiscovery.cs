using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Discoveries
{
    public interface IDiscovery
    {
        void Discover(ISettingsBuilder builder);
    }
}
