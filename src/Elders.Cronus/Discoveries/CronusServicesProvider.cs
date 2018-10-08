using Elders.Cronus.Serializer;

namespace Elders.Cronus.Discoveries
{
    public abstract class CronusServicesProvider
    {
        public void HandleDiscoveredModel(IDiscoveryResult<object> discoveredModel)
        {
            dynamic dynamicModel = (dynamic)discoveredModel;
            Handle(dynamicModel);
        }

        protected abstract void Handle(DiscoveryResult<ISerializer> discoveredModel);

        protected abstract void Handle(DiscoveryResult<ITransport> discoveredModel);
    }
}
