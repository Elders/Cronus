using System.Runtime.Serialization;
using System;

namespace Elders.Cronus.EventStore.AutoUpdater.Commands;

[DataContract(Name = "fc2cd66d-35a2-4713-ab6e-95c271331531")]
public class FailAutoUpdate : ISystemCommand
{
    FailAutoUpdate() { }

    public FailAutoUpdate(AutoUpdaterId id, DateTimeOffset timestamp)
    {
        Id = id;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public AutoUpdaterId Id { get; private set; }

    [DataMember(Order = 2)]
    public DateTimeOffset Timestamp { get; private set; }
}
