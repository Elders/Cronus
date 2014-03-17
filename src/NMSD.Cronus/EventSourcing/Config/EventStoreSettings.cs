using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipelining.Hosts.Config;

namespace NMSD.Cronus.EventSourcing.Config
{
    public abstract class EventStoreSettings
    {
        protected Assembly aggregateStatesAssembly;

        protected string boundedContext;

        protected bool createStorage;

        public CronusGlobalSettings GlobalSettings { get; set; }

        public abstract IEventStore Build();

        protected void BuildAggregateStatesAssemblyAndBoundedContext(Assembly aggregateStatesAssembly)
        {
            this.aggregateStatesAssembly = aggregateStatesAssembly;
            boundedContext = aggregateStatesAssembly.GetAssemblyAttribute<BoundedContextAttribute>().BoundedContextName;
        }
    }
}