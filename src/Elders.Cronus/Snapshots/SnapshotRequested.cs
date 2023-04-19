using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "7636607d-40b9-4dfe-b395-d2da71e02444")]
    public class SnapshotRequested : ISystemEvent
    {
        SnapshotRequested() { }

        public SnapshotRequested(SnapshotManagerId id, RevisionStatus newRevisionStatus, string contract, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(contract)) throw new ArgumentException($"'{nameof(contract)}' cannot be null or whitespace.", nameof(contract));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            NewRevisionStatus = newRevisionStatus;
            Contract = contract;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public RevisionStatus NewRevisionStatus { get; private set; }

        [DataMember(Order = 3)]
        public string Contract { get; private set; }

        [DataMember(Order = 4)]
        public DateTimeOffset Timestamp { get; private set; }
    }
}
