using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventSourcing
{
    [DataContract(Name = "f69daa12-171c-43a1-b049-be8a93ff137f")]
    public class AggregateCommit
    {
        AggregateCommit()
        {
            UncommittedEvents = new List<object>();
        }

        public AggregateCommit(IAggregateRootId aggregateId, int revision, List<IEvent> events)
        {
            AggregateId = aggregateId.RawId;
            Revision = revision;
            UncommittedEvents = events.Cast<object>().ToList();
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
        }

        [DataMember(Order = 1)]
        public byte[] AggregateId { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }

        [DataMember(Order = 3)]
        private List<object> UncommittedEvents { get; set; }

        [DataMember(Order = 4)]
        public long Timestamp { get; private set; }

        public List<IEvent> Events { get { return UncommittedEvents.Cast<IEvent>().ToList(); } }
    }
}