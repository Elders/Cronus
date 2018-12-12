using Microsoft.Extensions.Configuration;

namespace Elders.Cronus.Discoveries
{
    public interface ICronusServicesProvider
    {
        IConfiguration Configuration { get; }

        void HandleDiscoveredModel(IDiscoveryResult<object> discoveryResult);
    }
}