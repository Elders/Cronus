using Elders.Cronus.EventStore.Index;
using Elders.Cronus.Projections.Versioning;
using Elders.Cronus.Workflow;
using System;

namespace Elders.Cronus.MessageProcessing
{
    public class HandlerSubscriberFactory<T> : ISubscriberFactory<T>
    {
        private readonly Workflow<HandleContext> workflow;

        public HandlerSubscriberFactory(ISubscriberWorkflow<T> subscriberWorkflow)
        {
            workflow = subscriberWorkflow.GetWorkflow() as Workflow<HandleContext>;
        }

        public ISubscriber Create(Type handlerType)
        {
            return new HandlerSubscriber(handlerType, workflow);
        }
    }

    public class EventStoreIndexSubscriberFactory : ISubscriberFactory<IEventStoreIndex>
    {
        private readonly Workflow<HandleContext> workflow;
        private readonly TypeContainer<IEvent> allEventTypesInTheSystem;

        public EventStoreIndexSubscriberFactory(ISubscriberWorkflow<IEventStoreIndex> subscriberWorkflow, TypeContainer<IEvent> allEventTypesInTheSystem)
        {
            workflow = subscriberWorkflow.GetWorkflow() as Workflow<HandleContext>;
            this.allEventTypesInTheSystem = allEventTypesInTheSystem;
        }

        public ISubscriber Create(Type indexType)
        {
            return new EventStoreIndexSubscriber(indexType, allEventTypesInTheSystem, workflow);
        }
    }
}
