using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Commands;

[DataContract(Name = "c2a0bc37-9503-460b-a14f-a08dd7d1a755")]
public class FinishAutoUpdate : ISystemCommand
{
    FinishAutoUpdate() { }

    public FinishAutoUpdate(AutoUpdaterId id, string name, DateTimeOffset timestamp)
    {
        Id = id;
        Name = name;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public AutoUpdaterId Id { get; private set; }

    [DataMember(Order = 2)]
    public string Name { get; private set; }

    [DataMember(Order = 3)]
    public DateTimeOffset Timestamp { get; private set; }
}
