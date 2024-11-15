using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Events;

[DataContract(Name = "72fd104b-4141-4670-890b-1f877fd9521b")]
public class AutoUpdateFailed : ISystemEvent
{
    AutoUpdateFailed() { }

    public AutoUpdateFailed(AutoUpdaterId id, string name, uint sequence, string boundedContext, bool isSystem, DateTimeOffset timestamp)
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
