using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Tests.TestModel
{
    [DataContract(Name = "07cf4a30-a7b0-4a52-b6be-a6380ac14bf9")]
    public class TestSnapshotAggregateId : AggregateRootId
    {
        public TestSnapshotAggregateId(Guid id)
            : base("cronustest", "TestSnapshotAggregateId", id.ToString())
        {

        }

        public TestSnapshotAggregateId()
            : base("cronustest", "TestSnapshotAggregateId", Guid.NewGuid().ToString())
        {

        }
    }
}
