using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = EventStoreIndexSubscriber.ContractId)]
    public class EventStoreIndexSubscriber : SubscriberBase
    {
        public const string ContractId = "c8091ae7-a75a-4d66-a66b-de740f6bf9fd";

        private readonly IEnumerable<Type> events;
        private readonly IEnumerable<Type> publicEvents;

        public EventStoreIndexSubscriber(Type indexType, Workflow<HandleContext> indexWorkflow, IEnumerable<Type> events, IEnumerable<Type> publicEvents) : base(indexType, indexWorkflow)
        {
            this.events = events;
            this.publicEvents = publicEvents;
        }

        public override IEnumerable<Type> GetInvolvedMessageTypes()
        {
            return events.Concat(publicEvents);
        }
    }
}
