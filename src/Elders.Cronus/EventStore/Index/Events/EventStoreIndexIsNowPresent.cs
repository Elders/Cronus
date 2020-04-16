using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "3e1271c2-a9a3-44da-a4f9-82a6647d6ca2")]
    public class EventStoreIndexIsNowPresent : IEvent
    {
        EventStoreIndexIsNowPresent() { }

        public EventStoreIndexIsNowPresent(EventStoreIndexManagerId id)
        {
            Id = id;
            Timestamp = DateTimeOffset.UtcNow;
        }

        [DataMember(Order = 1)]
        public EventStoreIndexManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public DateTimeOffset Timestamp { get; private set; }

    }
}
