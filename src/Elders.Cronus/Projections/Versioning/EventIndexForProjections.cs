using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = EventStoreIndexSubscriber.ContractId)]
    public class EventStoreIndexSubscriber : SubscriberBase
    {
        public const string ContractId = "c8091ae7-a75a-4d66-a66b-de740f6bf9fd";

        private readonly TypeContainer<IEvent> allEventTypesInTheSystem;
        private readonly TypeContainer<IPublicEvent> allPublicEventTypesInTheSystem;

        public EventStoreIndexSubscriber(Type indexType, Workflow<HandleContext> indexWorkflow, TypeContainer<IEvent> allEventTypesInTheSystem, TypeContainer<IPublicEvent> allPublicEventTypesInTheSystem, ILogger<EventStoreIndexSubscriber> logger) : base(indexType, indexWorkflow, logger)
        {
            this.allEventTypesInTheSystem = allEventTypesInTheSystem;
            this.allPublicEventTypesInTheSystem = allPublicEventTypesInTheSystem;
        }

        public override IEnumerable<Type> GetInvolvedMessageTypes()
        {
            return allEventTypesInTheSystem.Items.Concat(allPublicEventTypesInTheSystem.Items);
        }
    }
}
