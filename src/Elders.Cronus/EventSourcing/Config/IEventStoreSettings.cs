using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.EventSourcing.Config
{
    public interface IEventStoreSettings : IHaveContainer, IHaveSerializer, ISettingsBuilder<IEventStore>
    {
        string BoundedContext { get; set; }
    }
}