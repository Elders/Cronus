using System.Reflection;
using NMSD.Cronus.Pipelining.Host.Config;

namespace NMSD.Cronus.EventSourcing.Config
{
    public class EventStoreSettings
    {
        public CronusGlobalSettings GlobalSettings { get; set; }

        public string BoundedContext { get; set; }

        public IEventStoreBuilder Builder { get; set; }

        public Assembly AggregateStatesAssembly { get; set; }
    }
}