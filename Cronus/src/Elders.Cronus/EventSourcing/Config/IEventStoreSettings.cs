using System.Reflection;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.EventSourcing.Config
{


    public interface IEventStoreSettings : IHaveSerializer, ISettingsBuilder<IEventStore>
    {
        string BoundedContext { get; set; }
        Assembly AggregateStatesAssembly { get; set; }
        Assembly DomainEventsAssembly { get; set; }
    }
}