using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Events;

[DataContract(Name = "adeff439-3624-4af7-a894-e2a19da863ae")]
public class AutoUpdateFinished : ISystemEvent
{
    AutoUpdateFinished() { }

    public AutoUpdateFinished(AutoUpdaterId id, uint currentVersion, string boundedContext, DateTimeOffset timestamp)
    {
        Id = id;
        BoundedContext = boundedContext;
        CurrentVersion = currentVersion;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public AutoUpdaterId Id { get; private set; }

    [DataMember(Order = 2)]
    public uint CurrentVersion { get; private set; }

    [DataMember(Order = 3)]
    public string BoundedContext { get; private set; }

    [DataMember(Order = 4)]
    public DateTimeOffset Timestamp { get; private set; }
}
