using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Namespace = "cronus", Name = "4f44a246-33a0-4314-aea2-80cd3b428b16")]
    public sealed class FinalizeEventStoreIndexRequest : ISystemCommand
    {
        FinalizeEventStoreIndexRequest()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public FinalizeEventStoreIndexRequest(EventStoreIndexManagerId id) : this()
        {
            Id = id;
        }

        [DataMember(Order = 1)]
        public EventStoreIndexManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public DateTimeOffset Timestamp { get; private set; }
    }
}
