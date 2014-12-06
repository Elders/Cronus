using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.EventStore
{
    [DataContract(Name = "f69daa12-171c-43a1-b049-be8a93ff137f")]
    public class AggregateCommit
    {
        AggregateCommit()
        {
            InternalEvents = new List<object>();
        }

        public AggregateCommit(IAggregateRootId aggregateId, int revision, List<IEvent> events)
        {
            AggregateRootId = aggregateId.RawId;
            BoundedContext = aggregateId.GetType().GetBoundedContext().BoundedContextName;
            Revision = revision;
            InternalEvents = events.Cast<object>().ToList();
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
        }

        [DataMember(Order = 1)]
        public byte[] AggregateRootId { get; private set; }

        [DataMember(Order = 2)]
        public string BoundedContext { get; private set; }

        [DataMember(Order = 3)]
        public int Revision { get; private set; }

        [DataMember(Order = 4)]
        private List<object> InternalEvents { get; set; }

        [DataMember(Order = 5)]
        public long Timestamp { get; private set; }

        public List<IEvent> Events { get { return InternalEvents.Cast<IEvent>().ToList(); } }
    }
}