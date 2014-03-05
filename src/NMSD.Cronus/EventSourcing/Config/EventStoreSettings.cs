using System.Reflection;

namespace NMSD.Cronus.Sample.Player
{
    public class EventStoreSettings
    {
        public string ConnectionString { get; set; }
        public string BoundedContext { get; set; }
        public Assembly AggregateStatesAssembly { get; set; }
    }
}