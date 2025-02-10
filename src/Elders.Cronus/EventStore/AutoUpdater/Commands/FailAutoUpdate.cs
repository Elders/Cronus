using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.EventStore.AutoUpdater.Commands;

[DataContract(Name = "fc2cd66d-35a2-4713-ab6e-95c271331531")]
public class FailAutoUpdate : ISystemCommand
{
    FailAutoUpdate() { }

    public FailAutoUpdate(AutoUpdaterId id, string name, DateTimeOffset timestamp)
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
