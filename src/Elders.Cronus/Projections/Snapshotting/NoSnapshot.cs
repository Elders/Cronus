using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Snapshotting
{
    [DataContract(Name = "78b01dba-1ced-450b-8990-d661da443179")]
    public class NoSnapshot : ISnapshot
    {
        public NoSnapshot() { }
        public NoSnapshot(IBlobId id, string projectionName)
        {
            Id = id;
            ProjectionName = projectionName;
        }

        [DataMember(Order = 1)]
        public IBlobId Id { get; set; }

        [DataMember(Order = 2)]
        public string ProjectionName { get; set; }

        [DataMember(Order = 3)]
        public int Revision { get { return -1; } }

        [DataMember(Order = 4)]
        public object State { get; private set; }

        public void InitializeState(object state) { }

        public SnapshotMeta GetMeta()
        {
            return new SnapshotMeta(Revision, ProjectionName);
        }
    }
}
