using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index;

[DataContract(Name = "f9c1c15b-ed60-4fa8-ab5f-251403aa8681")]
public sealed class RebuildIndexCommand : ISystemCommand
{
    RebuildIndexCommand()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    public RebuildIndexCommand(EventStoreIndexManagerId id) : this()
    {
        if (id is null) throw new ArgumentNullException(nameof(id));

        Id = id;
    }

    public RebuildIndexCommand(EventStoreIndexManagerId id, int? maxDegreeOfParallelism) : this()
    {
        if (id is null) throw new ArgumentNullException(nameof(id));

        Id = id;
        MaxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    [DataMember(Order = 1)]
    public EventStoreIndexManagerId Id { get; private set; }

    [DataMember(Order = 2)]
    public DateTimeOffset Timestamp { get; private set; }

    [DataMember(Order = 3)]
    public int? MaxDegreeOfParallelism { get; private set; }
}
