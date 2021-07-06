using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = EventStoreIndexSubscriber.ContractId)]
    public class EventStoreIndexSubscriber : SubscriberBase
    {
        public const string ContractId = "c8091ae7-a75a-4d66-a66b-de740f6bf9fd";

        private readonly TypeContainer<IEvent> allEventTypesInTheSystem;

        public EventStoreIndexSubscriber(Type indexType, Workflow<HandleContext> indexWorkflow, TypeContainer<IEvent> allEventTypesInTheSystem) : base(indexType, indexWorkflow)
        {
            this.allEventTypesInTheSystem = allEventTypesInTheSystem;
        }

        public override IEnumerable<Type> GetInvolvedMessageTypes()
        {
            return allEventTypesInTheSystem.Items;
        }
    }
}
