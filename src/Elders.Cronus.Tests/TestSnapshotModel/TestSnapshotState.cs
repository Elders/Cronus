using System.Runtime.Serialization;

namespace Elders.Cronus.Tests.TestModel
{
    [DataContract(Name = "ce1b64ee-6823-4313-8004-820932e6170f")]
    public class TestSnapshotState
    {
        [DataMember(Order = 1)]
        public TestSnapshotAggregateId Id { get; set; }

        [DataMember(Order = 2)]
        public string UpdatableField { get; set; }
    }
}
