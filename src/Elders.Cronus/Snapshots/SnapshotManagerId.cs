using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "ea31f8b0-88c5-4b98-8666-99465ba155f9")]
    public class SnapshotManagerId : AggregateRootId
    {
        SnapshotManagerId() { }
        public SnapshotManagerId(Urn instanceId, string tenant) : base(tenant, "snapshot", instanceId.NSS)
        {
            InstanceId = instanceId;
        }

        [DataMember(Order = 1)]
        public Urn InstanceId { get; private set; }
    }
}
