using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.EventSourcing.Config
{
    public abstract class EventStoreSettings
    {
        protected Assembly aggregateStatesAssembly;

        protected Assembly domainEventsAssembly;

        public string BoundedContext { get; protected set; }

        protected bool createStorage;

        public CronusGlobalSettings GlobalSettings { get; set; }

        public abstract IAggregateRepository BuildAggregateRepository();

        public abstract IEventStorePersister BuildEventStorePersister();

        public abstract IMessageProcessor<DomainMessageCommit> BuildEventStoreHandlers();

        public abstract IEventStorePlayer BuildEventStorePlayer();

        protected void BuildAggregateStatesAssemblyAndBoundedContext(Assembly aggregateStatesAssembly)
        {
            this.aggregateStatesAssembly = aggregateStatesAssembly;
            BoundedContext = aggregateStatesAssembly.GetAssemblyAttribute<BoundedContextAttribute>().BoundedContextName;
        }
    }
}