using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Commands;

[DataContract(Name = "55fefc22-9c67-4f2d-84a9-c1e65df545a9")]
public class RequestAutoUpdate : ISystemCommand
{
    RequestAutoUpdate() { }

    public RequestAutoUpdate(AutoUpdaterId id, uint majorVersion, string boundedContext, DateTimeOffset timestamp)
    {
        Id = id;
        MajorVersion = majorVersion;
        BoundedContext = boundedContext;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public AutoUpdaterId Id { get; private set; }

    [DataMember(Order = 2)]
    public uint MajorVersion { get; private set; }

    [DataMember(Order = 3)]
    public string BoundedContext { get; private set; }

    [DataMember(Order = 4)]
    public DateTimeOffset Timestamp { get; private set; }
}
