using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index;

[DataContract(Name = "64ed4d38-d197-4bdd-b0eb-a1f740c33832")]
public sealed class RegisterIndex : ISystemCommand
{
    RegisterIndex()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    public RegisterIndex(EventStoreIndexManagerId id) : this()
    {
        if (id is null) throw new ArgumentNullException(nameof(id));

        Id = id;
    }

    [DataMember(Order = 1)]
    public EventStoreIndexManagerId Id { get; private set; }

    [DataMember(Order = 2)]
    public DateTimeOffset Timestamp { get; private set; }
}
