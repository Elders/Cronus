using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Commands;

[DataContract(Name = "2071245a-d588-4fd9-aabf-c3729cf2e692")]
public class BulkRequestAutoUpdate : ISystemCommand
{
    BulkRequestAutoUpdate()
    {
        AutoUpdates = new List<SingleAutoUpdate>();
        Timestamp = DateTimeOffset.UtcNow;
    }

    public BulkRequestAutoUpdate(AutoUpdaterId id, string boundedContext, IEnumerable<SingleAutoUpdate> autoUpdates, DateTimeOffset timestamp) : this()
    {
        Id = id;
        BoundedContext = boundedContext;
        AutoUpdates = autoUpdates;
    }

    [DataMember(Order = 1)]
    public AutoUpdaterId Id { get; private set; }

    [DataMember(Order = 2)]
    public string BoundedContext { get; private set; }

    [DataMember(Order = 3)]
    public IEnumerable<SingleAutoUpdate> AutoUpdates { get; private set; }

    [DataMember(Order = 4)]
    public DateTimeOffset Timestamp { get; private set; }
}


[DataContract(Name = "e02bedb5-2bed-46ad-a1a7-f65eae152035")]
public record class SingleAutoUpdate
{
    SingleAutoUpdate() { }

    public SingleAutoUpdate(string name, uint sequence, bool isSystem)
    {
        Name = name;
        Sequence = sequence;
        IsSystem = isSystem;
    }

    [DataMember(Order = 1)]
    public string Name { get; private set; }

    [DataMember(Order = 2)]
    public uint Sequence { get; private set; }

    [DataMember(Order = 3)]
    public bool IsSystem { get; private set; }
}
