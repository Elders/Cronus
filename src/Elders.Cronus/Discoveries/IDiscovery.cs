namespace Elders.Cronus.Discoveries;

public interface IDiscovery<out TCronusService>    //where TCronusService : ICronusService
{
    string Name { get; }

    IDiscoveryResult<TCronusService> Discover(DiscoveryContext context);
}
