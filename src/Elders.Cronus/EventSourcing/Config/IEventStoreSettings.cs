using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.EventSourcing.Config
{
    public interface IEventStoreSettings : ISettingsBuilder
    {
        string BoundedContext { get; set; }
    }
}