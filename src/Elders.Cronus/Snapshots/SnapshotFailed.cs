using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "40f98848-7af4-4108-b075-1f95d90c06f0")]
    public class SnapshotFailed : ISystemEvent
    {
        SnapshotFailed() { }

        public SnapshotFailed(SnapshotManagerId id, RevisionStatus newRevisionStatus, RevisionStatus previousRevisionStatus, string contract, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(contract)) throw new ArgumentException($"'{nameof(contract)}' cannot be null or whitespace.", nameof(contract));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            NewRevisionStatus = newRevisionStatus ?? throw new ArgumentNullException(nameof(newRevisionStatus));
            PreviousRevisionStatus = previousRevisionStatus;
            Contract = contract;
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public RevisionStatus NewRevisionStatus { get; private set; }

        [DataMember(Order = 3)]
        public RevisionStatus PreviousRevisionStatus { get; private set; }

        [DataMember(Order = 4)]
        public string Contract { get; private set; }

        [DataMember(Order = 5)]
        public DateTimeOffset Timestamp { get; private set; }
    }
}
