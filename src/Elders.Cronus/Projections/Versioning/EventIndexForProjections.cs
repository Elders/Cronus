using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = EventStoreIndexSubscriber.ContractId)]
    public class EventStoreIndexSubscriber : ISubscriber
    {
        public const string ContractId = "c8091ae7-a75a-4d66-a66b-de740f6bf9fd";

        protected readonly Workflow<HandleContext> handlerWorkflow;

        protected readonly Type handlerType;
        private readonly TypeContainer<IEvent> allEventTypesInTheSystem;

        public EventStoreIndexSubscriber(Type indexType, TypeContainer<IEvent> allEventTypesInTheSystem, Workflow<HandleContext> indexWorkflow, string subscriberId = null)
        {
            if (indexType is null) throw new ArgumentNullException(nameof(indexType));
            if (indexWorkflow is null) throw new ArgumentNullException(nameof(indexWorkflow));

            this.handlerType = indexType;
            this.allEventTypesInTheSystem = allEventTypesInTheSystem;
            this.handlerWorkflow = indexWorkflow;
            Id = subscriberId ?? indexType.FullName;
        }

        public string Id { get; private set; }

        public virtual void Process(CronusMessage message)
        {
            var context = new HandleContext(message, handlerType);
            handlerWorkflow.Run(context);
        }

        public virtual IEnumerable<Type> GetInvolvedMessageTypes()
        {
            return allEventTypesInTheSystem.Items;
        }
    }
}
