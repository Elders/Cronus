using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "9047662e-2be2-4df6-98c6-0efc79baf0a4")]
    public class CancelSnapshot : ISystemCommand
    {
        CancelSnapshot() { }

        public CancelSnapshot(SnapshotManagerId id, int revision)
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
