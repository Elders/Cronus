namespace Elders.Cronus.Discoveries
{
    public abstract class DiscoveryBase<TCronusService> : IDiscovery<TCronusService>    //where TCronusService : ICronusService
    {
        public virtual string Name { get { return this.GetType().Name; } }

        public IDiscoveryResult<TCronusService> Discover(DiscoveryContext context)
        {
            return DiscoverFromAssemblies(context);
        }

        protected abstract DiscoveryResult<TCronusService> DiscoverFromAssemblies(DiscoveryContext context);
    }
}
