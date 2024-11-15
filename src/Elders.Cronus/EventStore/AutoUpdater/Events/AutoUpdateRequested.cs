using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Events;

[DataContract(Name = "2150b60e-7ce4-4d40-bf44-556e1c53e8a1")]
public class AutoUpdateRequested : ISystemEvent
{
    AutoUpdateRequested() { }

    public AutoUpdateRequested(AutoUpdaterId id, string name, uint sequence, string boundedContext, bool isSystem, DateTimeOffset timestamp)
    {
        Id = id;
        Name = name;
        Sequence = sequence;
        BoundedContext = boundedContext;
        IsSystem = isSystem;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public AutoUpdaterId Id { get; private set; }

    [DataMember(Order = 2)]
    public string Name { get; private set; }

    [DataMember(Order = 3)]
    public uint Sequence { get; private set; }

    [DataMember(Order = 4)]
    public string BoundedContext { get; private set; }

    [DataMember(Order = 5)]
    public bool IsSystem { get; private set; }

    [DataMember(Order = 6)]
    public DateTimeOffset Timestamp { get; private set; }
}
