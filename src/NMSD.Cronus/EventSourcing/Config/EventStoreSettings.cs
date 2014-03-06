using System.Reflection;

namespace NMSD.Cronus.EventSourcing.Config
{
    public class EventStoreSettings
    {
        public string ConnectionString { get; set; }
        public string BoundedContext { get; set; }
        public Assembly AggregateStatesAssembly { get; set; }
    }
}