using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Events;

[DataContract(Name = "adeff439-3624-4af7-a894-e2a19da863ae")]
public class AutoUpdateFinished : ISystemEvent
{
    AutoUpdateFinished() { }
    public AutoUpdateFinished(AutoUpdaterId id, string name, uint sequence, string boundedContext, bool isSystem, DateTimeOffset timestamp)
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
