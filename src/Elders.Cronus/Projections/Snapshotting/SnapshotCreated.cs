using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Snapshotting
{
    [DataContract(Name = "f9cd40cc-5fd1-48da-a09a-996cd46baf92")]
    public class SnapshotCreated : ISystemEvent
    {
        public SnapshotCreated(SnapshotId snapshotId, ISnapshot snapshot)
        {
            SnapshotId = snapshotId;
            Snapshot = snapshot;
        }

        [DataMember(Order = 1)]
        public SnapshotId SnapshotId { get; set; }

        [DataMember(Order = 2)]
        public ISnapshot Snapshot { get; set; }
    }

    [DataContract(Name = "cf66a94e-bb04-4f34-87cb-74a8d6b2e5a7")]
    public class CreateSnapshot : ISystemCommand
    {
        public CreateSnapshot(SnapshotId snapshotId, ISnapshot snapshot)
        {
            SnapshotId = snapshotId;
            Snapshot = snapshot;
        }

        [DataMember(Order = 1)]
        public SnapshotId SnapshotId { get; set; }

        [DataMember(Order = 2)]
        public ISnapshot Snapshot { get; set; }
    }

    [DataContract(Name = "d890321e-b8a5-4860-85ef-95011f3a168e")]
    public class SnapshotId : AggregateRootId
    {
        SnapshotId() : base() { }

        public SnapshotId(string projectionName, string tenant) : base(projectionName, "snapshot", tenant) { }
    }
}
