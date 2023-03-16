using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "364e7b95-5715-46b9-bd2c-6849ec534072")]
    public class RevisionStatus : ValueObject<RevisionStatus>
    {
        RevisionStatus() { }

        public RevisionStatus(int revision, SnapshotRevisionStatus status, DateTimeOffset timestamp)
        {
            Revision = revision;
            Status = status ?? throw new ArgumentNullException(nameof(status));
            Timestamp = timestamp;
        }

        [DataMember(Order = 1)]
        public int Revision { get; private set; }

        [DataMember(Order = 2)]
        public SnapshotRevisionStatus Status { get; private set; }

        [DataMember(Order = 3)]
        public DateTimeOffset Timestamp { get; private set; }
    }
}
