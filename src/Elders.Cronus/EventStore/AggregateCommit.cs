using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore
{
    [DataContract(Name = "f69daa12-171c-43a1-b049-be8a93ff137f")]
    public class AggregateCommit
    {
        AggregateCommit()
        {
            Events = new List<IEvent>();
        }

        public AggregateCommit(IBlobId aggregateId, int revision, List<IEvent> events)
            : this(aggregateId.RawId, aggregateId.GetType().GetBoundedContext().BoundedContextName, revision, events)
        { }

        public AggregateCommit(byte[] aggregateRootId, string boundedContext, int revision, List<IEvent> events)
        {
            AggregateRootId = aggregateRootId;
            BoundedContext = boundedContext;
            Revision = revision;
            Events = events;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
        }

        public AggregateCommit(byte[] aggregateRootId, string boundedContext, int revision, List<IEvent> events, long timestamp)
        {
            AggregateRootId = aggregateRootId;
            BoundedContext = boundedContext;
            Revision = revision;
            Events = events;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public byte[] AggregateRootId { get; private set; }

        [DataMember(Order = 2)]
        public string BoundedContext { get; private set; }

        [DataMember(Order = 3)]
        public int Revision { get; private set; }

        [DataMember(Order = 4)]
        public List<IEvent> Events { get; set; }

        [DataMember(Order = 5)]
        public long Timestamp { get; private set; }
    }
}
