using System;
using System.Runtime.Serialization;
using Elders.Cronus.Projections.Versioning;

namespace Elders.Cronus.EventStore.Index;

[DataContract(Name = "b57d0dd4-4c86-4f8f-9247-fdb71820095a")]
public sealed class EventStoreIndexRequested : ISystemEvent
{
    EventStoreIndexRequested() { }

    public EventStoreIndexRequested(EventStoreIndexManagerId id, DateTimeOffset requestTimestamp, VersionRequestTimebox timebox, int maxDegreeOfParallelism)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Timestamp = requestTimestamp;
        Timebox = timebox ?? throw new ArgumentNullException(nameof(timebox));
        MaxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    [DataMember(Order = 1)]
    public EventStoreIndexManagerId Id { get; private set; }

    [DataMember(Order = 2)]
    public DateTimeOffset Timestamp { get; private set; }

    [DataMember(Order = 3)]
    public VersionRequestTimebox Timebox { get; private set; }

    [DataMember(Order = 4)]
    public int MaxDegreeOfParallelism { get; private set; }
}
