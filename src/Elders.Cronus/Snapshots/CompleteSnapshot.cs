using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "c29ed844-58da-4196-81f3-96b29ad1628f")]
    public class CompleteSnapshot : ISystemCommand
    {
        CompleteSnapshot() { }

        public CompleteSnapshot(SnapshotManagerId id, int revision)
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
