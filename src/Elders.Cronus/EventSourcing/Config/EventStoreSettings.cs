using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.EventSourcing.Config
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