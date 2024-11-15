using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Commands;

[DataContract(Name = "6c4dc36c-2e4e-45d5-a9de-fd207907e4b7")]
public class RequestAutoUpdate : ISystemCommand
{
    RequestAutoUpdate() { }

    public RequestAutoUpdate(AutoUpdaterId id, string boundedContext, SingleAutoUpdate autoUpdate, DateTimeOffset timestamp)
    {
        Id = id;
        BoundedContext = boundedContext;
        AutoUpdate = autoUpdate;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public AutoUpdaterId Id { get; private set; }

    [DataMember(Order = 2)]
    public string BoundedContext { get; private set; }

    [DataMember(Order = 3)]
    public SingleAutoUpdate AutoUpdate { get; private set; }

    [DataMember(Order = 4)]
    public DateTimeOffset Timestamp { get; private set; }
}
