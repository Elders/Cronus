using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "364e7b95-5715-46b9-bd2c-6849ec534072")]
    public class RevisionStatus : ValueObject<RevisionStatus>
    {
        RevisionStatus() { }

        public RevisionStatus(int revision, SnapshotStatus status)
        {
            Revision = revision;
            Status = status ?? throw new ArgumentNullException(nameof(status));
        }

        [DataMember(Order = 1)]
        public int Revision { get; private set; }

        [DataMember(Order = 2)]
        public SnapshotStatus Status { get; private set; }
    }
}
