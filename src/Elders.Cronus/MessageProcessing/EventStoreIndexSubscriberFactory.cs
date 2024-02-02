using System;
using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Projections.Versioning;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    public class EventStoreIndexSubscriberFactory<TIndex> : ISubscriberFactory<TIndex>
        where TIndex : IEventStoreIndex
    {
        private readonly Workflow<HandleContext> workflow;
        private readonly TypeContainer<IEvent> allEventTypesInTheSystem;
        private readonly TypeContainer<IPublicEvent> allPublicEventTypesInTheSystem;

        public EventStoreIndexSubscriberFactory(ISubscriberWorkflowFactory<TIndex> subscriberWorkflow, TypeContainer<IEvent> allEventTypesInTheSystem, TypeContainer<IPublicEvent> allPublicEventTypesInTheSystem)
        {
            workflow = subscriberWorkflow.GetWorkflow() as Workflow<HandleContext>;
            this.allEventTypesInTheSystem = allEventTypesInTheSystem;
            this.allPublicEventTypesInTheSystem = allPublicEventTypesInTheSystem;
        }

        public ISubscriber Create(Type indexType)
        {
            return new EventStoreIndexSubscriber(indexType, workflow, allEventTypesInTheSystem, allPublicEventTypesInTheSystem);
        }
    }
}
