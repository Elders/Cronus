using Elders.Cronus.Projections.Versioning;
using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "b57d0dd4-4c86-4f8f-9247-fdb71820095a")]
    public sealed class EventStoreIndexRequested : ISystemEvent
    {
        EventStoreIndexRequested() { }

        public EventStoreIndexRequested(EventStoreIndexManagerId id, DateTimeOffset requestTimestamp, VersionRequestTimebox timebox)
        {
            Id = id;
            Timestamp = requestTimestamp;
            Timebox = timebox;
        }

        [DataMember(Order = 1)]
        public EventStoreIndexManagerId Id { get; set; }

        [DataMember(Order = 2)]
        public DateTimeOffset Timestamp { get; private set; }

        [DataMember(Order = 3)]
        public VersionRequestTimebox Timebox { get; private set; }
    }
}
