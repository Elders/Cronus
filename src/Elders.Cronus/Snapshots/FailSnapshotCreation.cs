using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "b8af24bb-598c-4683-9416-08c120514b62")]
    public class FailSnapshotCreation : ISystemCommand
    {
        FailSnapshotCreation() { }

        public FailSnapshotCreation(SnapshotManagerId id, int revision)
        {
            Id = id;
            Revision = revision;
        }

        [DataMember(Order = 1)]
        public SnapshotManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public int Revision { get; private set; }
    }
}
