using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Events;

[DataContract(Name = "9c3a9f5c-0a91-4b1d-82d1-4f3409913046")]
public class AutoUpdateTriggered : ISystemEvent
{
    AutoUpdateTriggered() { }

    public AutoUpdateTriggered(AutoUpdaterId id, uint previousVersion , uint currentVersion, string boundedContext, DateTimeOffset timestamp)
    {
        Id = id;
        BoundedContext = boundedContext;
        PreviousVersion = previousVersion;
        CurrentVersion = currentVersion;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public AutoUpdaterId Id { get; private set; }

    [DataMember(Order = 2)]
    public uint PreviousVersion { get; private set; }

    [DataMember(Order = 3)]
    public uint CurrentVersion { get; private set; }

    [DataMember(Order = 4)]
    public string BoundedContext { get; private set; }

    [DataMember(Order = 5)]
    public DateTimeOffset Timestamp { get; private set; }
}
