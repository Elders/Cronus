using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.DomainModelling
{
    [DataContract(Name = "4c08f48c-5cb0-456d-b62b-608059a1e78d")]
    public abstract class Command : ICommand
    {
        protected Command() { }

        protected Command(IAggregateRootId aggregateRootId)
        {
            MetaAggregateIdRaw = aggregateRootId;
            MetaCommandId = Guid.NewGuid();
            MetaTimestamp = DateTime.UtcNow.ToFileTimeUtc();
            MetaExpectedAggregateRevision = -1; //  We did not request a revision number from AggregateRevisioningService.
        }

        public IAggregateRootId MetaAggregateId { get { return (IAggregateRootId)MetaAggregateIdRaw; } }

        [DataMember(Order = 1000)]
        private object MetaAggregateIdRaw;

        [DataMember(Order = 1001)]
        public long MetaTimestamp { get; private set; }

        [DataMember(Order = 1002)]
        public Guid MetaCommandId { get; private set; }

        [DataMember(Order = 1003)]
        public int MetaExpectedAggregateRevision { get; set; }

    }

    [DataContract(Name = "5d6572eb-a5f8-407e-8b7e-482d03eecb49")]
    public abstract class Event : IEvent
    {
        protected Event() { }

        protected Event(IAggregateRootId aggregateRootId)
        {
            MetaAggregateIdRaw = aggregateRootId;
            MetaTimestamp = DateTime.UtcNow.ToFileTimeUtc();
        }

        public IAggregateRootId MetaAggregateId { get { return (IAggregateRootId)MetaAggregateIdRaw; } }

        [DataMember(Order = 1000)]
        private object MetaAggregateIdRaw;

        [DataMember(Order = 1001)]
        public long MetaTimestamp { get; private set; }
    }
}