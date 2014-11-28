using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.EventStore.Config
{
    public interface IEventStoreSettings : ISettingsBuilder
    {
        string BoundedContext { get; set; }
    }
}