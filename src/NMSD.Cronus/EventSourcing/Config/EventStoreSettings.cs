using System.Reflection;
using NMSD.Cronus.Pipelining.Host.Config;

namespace NMSD.Cronus.EventSourcing.Config
{
    public abstract class EventStoreSettings
    {
        public CronusGlobalSettings GlobalSettings { get; set; }

        public string BoundedContext { get; set; }

        public abstract IEventStore Build();

        public Assembly AggregateStatesAssembly { get; set; }
    }
}