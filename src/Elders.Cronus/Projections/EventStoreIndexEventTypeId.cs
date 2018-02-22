using System;
using System.Text;
using System.Runtime.Serialization;
using Elders.Cronus.Middleware;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Projections
{
    [DataContract(Name = "3488c890-bc51-4eca-8d44-d2c486aec7a2")]
    public class EventStoreIndexEventTypeId : IBlobId
    {
        [DataMember(Order = 1)]
        private string contractId;

        EventStoreIndexEventTypeId() { }

        public EventStoreIndexEventTypeId(string contractId)
        {
            if (string.IsNullOrEmpty(contractId)) throw new ArgumentNullException(nameof(contractId));

            this.contractId = contractId;
        }

        public EventStoreIndexEventTypeId(Type eventType) : this(eventType.GetContractId()) { }

        public byte[] RawId { get { return Encoding.UTF8.GetBytes(contractId); } }
    }

    [DataContract(Name = "3cc90b7e-56b3-4566-b4ae-d1523d203b20")]
    public class EventStoreIndexEventStateTypeId : IBlobId
    {
        [DataMember(Order = 1)]
        private string contractId;

        EventStoreIndexEventStateTypeId()
        { }

        public EventStoreIndexEventStateTypeId(string contractId)
        {
            if (string.IsNullOrEmpty(contractId)) throw new ArgumentNullException(nameof(contractId));

            this.contractId = contractId;
        }

        public EventStoreIndexEventStateTypeId(Type eventType) : this(eventType.GetContractId()) { }

        public byte[] RawId { get { return Encoding.UTF8.GetBytes(contractId); } }
    }

    public class EventSourcedProjectionsMiddleware : Middleware<HandleContext>
    {
        readonly IProjectionRepository repository;

        public EventSourcedProjectionsMiddleware(IProjectionRepository repository)
        {
            if (ReferenceEquals(null, repository) == true) throw new ArgumentNullException(nameof(repository));

            this.repository = repository;
        }

        protected override void Run(Execution<HandleContext> execution)
        {
            CronusMessage cronusMessage = execution.Context.Message;
            if (cronusMessage.Payload is IEvent)
            {
                Type projectionType = execution.Context.HandlerType;
                repository.Save(projectionType, cronusMessage);
            }
        }
    }
}
