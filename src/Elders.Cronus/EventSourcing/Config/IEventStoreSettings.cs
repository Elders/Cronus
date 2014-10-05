using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.EventSourcing.Config
{
    public interface IEventStoreSettings : IHaveSerializer, ISettingsBuilder<IEventStore>
    {
        string BoundedContext { get; set; }
    }
}