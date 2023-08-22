namespace Elders.Cronus.Discoveries;

public interface ICronusServicesProvider
{
    void HandleDiscoveredModel(IDiscoveryResult<object> discoveryResult);
}
