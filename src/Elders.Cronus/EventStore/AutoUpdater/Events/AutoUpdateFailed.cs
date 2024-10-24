using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Events;

[DataContract(Name = "72fd104b-4141-4670-890b-1f877fd9521b")]
public class AutoUpdateFailed : ISystemEvent
{
    AutoUpdateFailed() { }

    public AutoUpdateFailed(AutoUpdaterId id, uint currentVersion, string boundedContext, DateTimeOffset timestamp)
    {
        Id = id;
        BoundedContext = boundedContext;
        FailedVersion = currentVersion;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public AutoUpdaterId Id { get; private set; }

    [DataMember(Order = 3)]
    public uint FailedVersion { get; private set; }

    [DataMember(Order = 4)]
    public string BoundedContext { get; private set; }

    [DataMember(Order = 5)]
    public DateTimeOffset Timestamp { get; private set; }
}
