using Elders.Cronus.Serializer;

namespace Elders.Cronus.Discoveries
{
    public abstract class CronusServices
    {
        internal void Handle(DiscoveryResult result)
        {
            dynamic instance = (dynamic)result.Model;
            Handle(instance);
        }

        protected abstract void Handle(ISerializer serializer);
        protected abstract void Handle(ITransport transport);
    }
}
