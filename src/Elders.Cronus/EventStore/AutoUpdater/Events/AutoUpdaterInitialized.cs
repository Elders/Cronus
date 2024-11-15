using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater.Events;

[DataContract(Name = "47afc409-d845-46c6-834a-7fd6edd951f0")]
public class AutoUpdaterInitialized : ISystemEvent
{
    AutoUpdaterInitialized() { }

    public AutoUpdaterInitialized(AutoUpdaterId id, string boundedContext, DateTimeOffset timestamp)
    {
        Id = id;
        BoundedContext = boundedContext;
        Timestamp = timestamp;
    }

    [DataMember(Order = 1)]
    public AutoUpdaterId Id { get; private set; }

    [DataMember(Order = 2)]
    public string BoundedContext { get; private set; }

    [DataMember(Order = 3)]
    public DateTimeOffset Timestamp { get; private set; }
}
