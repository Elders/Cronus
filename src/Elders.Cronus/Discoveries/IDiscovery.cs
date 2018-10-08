namespace Elders.Cronus.Discoveries
{
    public interface IDiscovery<out T>
    {
        string Name { get; }

        IDiscoveryResult<T> Discover();
    }
}
