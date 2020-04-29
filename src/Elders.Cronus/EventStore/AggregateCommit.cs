using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore
{
    [DataContract(Name = "f69daa12-171c-43a1-b049-be8a93ff137f")]
    public class AggregateCommit
    {
        AggregateCommit()
        {
            Events = new List<IEvent>();
            PublicEvents = new List<IPublicEvent>();
        }

        public AggregateCommit(IBlobId aggregateId, int revision, List<IEvent> events) : this(aggregateId.RawId, revision, events, new List<IPublicEvent>()) { }
        public AggregateCommit(byte[] aggregateRootId, int revision, List<IEvent> events) : this(aggregateRootId, revision, events, new List<IPublicEvent>(), DateTime.UtcNow.ToFileTimeUtc()) { }

        public AggregateCommit(IBlobId aggregateId, int revision, List<IEvent> events, List<IPublicEvent> publicEvents) : this(aggregateId.RawId, revision, events, publicEvents) { }
        public AggregateCommit(byte[] aggregateRootId, int revision, List<IEvent> events, List<IPublicEvent> publicEvents) : this(aggregateRootId, revision, events, publicEvents, DateTime.UtcNow.ToFileTimeUtc()) { }

        public AggregateCommit(byte[] aggregateRootId, int revision, List<IEvent> events, List<IPublicEvent> publicEvents, long timestamp)
        {
            AggregateRootId = aggregateRootId;
            Revision = revision;
            Events = events;
            PublicEvents = publicEvents;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public byte[] AggregateRootId { get; private set; }

        [Obsolete]
        [DataMember(Order = 2)]
        string BoundedContext { get; set; }

        [DataMember(Order = 3)]
        public int Revision { get; private set; }

        [DataMember(Order = 4)]
        public List<IEvent> Events { get; set; }

        [DataMember(Order = 6)]
        public List<IPublicEvent> PublicEvents { get; set; }

        [DataMember(Order = 5)]
        public long Timestamp { get; private set; }

        public override string ToString()
        {
            string commitInfo =
                "AggregateCommit details" + Environment.NewLine +
                "RootId:" + AggregateRootId.ToString() + Environment.NewLine +
                "Revision:" + Revision + Environment.NewLine +
                "Events:" + string.Join(Environment.NewLine, Events.Select(e => "\t" + e.ToString())) +
                "PublicEvents:" + string.Join(Environment.NewLine, PublicEvents.Select(e => "\t" + e.ToString()));

            return commitInfo;
        }
    }
}
